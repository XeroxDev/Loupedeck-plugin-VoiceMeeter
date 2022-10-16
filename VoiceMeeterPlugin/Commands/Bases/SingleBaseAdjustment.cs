namespace Loupedeck.VoiceMeeterPlugin.Commands.Bases
{
    using System;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading.Tasks;

    using Extensions;

    using Helper;

    using Library.Voicemeeter;

    using Services;

    public class SingleBaseAdjustment : PluginDynamicAdjustment
    {
        private AdjustmentItem[] Actions { get; set; }
        private VoiceMeeterService VmService { get; }
        private String Command { get; set; }
        private Subject<Boolean> OnDestroy { get; } = new();
        private Int32 Offset { get; set; }
        private Boolean IsStrip { get; }
        private Int32 MaxValue { get; }
        private Int32 MinValue { get; }
        private Int32 ScaleFactor { get; }
        public Boolean IsRealClass { get; set; }
        protected Boolean Loaded { get; set; }

        public SingleBaseAdjustment(Boolean hasRestart, Boolean isRealClass, Boolean isStrip, Int32 minValue = 0,
            Int32 maxValue = 10, Int32 scaleFactor = 1) : base(hasRestart)
        {
            this.IsRealClass = isRealClass;
            this.IsStrip = isStrip;

            this.MinValue = minValue;
            this.MaxValue = maxValue;
            this.ScaleFactor = scaleFactor;

            if (!this.IsRealClass)
            {
                return;
            }

            this.VmService = VoiceMeeterService.Instance;
        }

        public async Task CreateCommands(Int32 stripCount, String cmd, Int32 offset, String displayName = null)
        {
            this.Command = cmd;
            this.Offset = offset;
            this.DisplayName = displayName ?? cmd;
            while (!this.VmService.Connected)
            {
                await Task.Delay(1000);
            }

            this.Actions = new AdjustmentItem[stripCount];
            for (var hi = 0; hi < stripCount; hi++)
            {
                var name = Remote.GetTextParameter($"{(this.IsStrip ? "Strip" : "Bus")}[{hi + this.Offset}].Label");
                var groupName = String.IsNullOrEmpty(name) ? this.IsStrip ? "Strip" : "Bus" : name;
                this.AddParameter(
                    GetActionParameterName(hi, cmd),
                    this.DisplayName,
                    $"{groupName} ({hi + 1 + this.Offset})",
                    "Input"
                );
            }

            this.GetNewSettings();
        }

        private static String GetActionParameterName(Int32 stripNumber, String cmd) => $"VM-Strip{stripNumber}-{cmd}";

        private void GetNewSettings()
        {
            for (var hiIndex = 0; hiIndex < this.Actions.Length; hiIndex++)
            {
                var oldItem = this.Actions[hiIndex] ?? new AdjustmentItem();

                var param = $"{(this.IsStrip ? "Strip" : "Bus")}[{hiIndex + this.Offset}]";

                var newItem = new AdjustmentItem
                {
                    Value = (Int32)(Remote.GetParameter($"{param}.{this.Command}") * this.ScaleFactor),
                    Name = Remote.GetTextParameter($"{param}.Label"),
                    IsMuted = Remote.GetParameter($"{param}.Mute") > 0,
                };

                this.Actions[hiIndex] = newItem;

                if (this.Loaded && (oldItem.Value != newItem.Value || oldItem.Name != newItem.Name || oldItem.IsMuted != newItem.IsMuted))
                {
                    this.AdjustmentValueChanged(GetActionParameterName(hiIndex, this.Command));
                }
            }
        }

        protected override Boolean OnLoad()
        {
            this.Loaded = true;
            if (!this.IsRealClass)
            {
                return base.OnLoad();
            }

            this.VmService.Parameters
                .TakeUntil(this.OnDestroy)
                .Subscribe(_ => this.GetNewSettings());

            return base.OnLoad();
        }

        protected override Boolean OnUnload()
        {
            if (!this.IsRealClass)
            {
                return base.OnLoad();
            }

            this.OnDestroy.OnNext(true);
            return base.OnUnload();
        }

        protected override void RunCommand(String actionParameter)
        {
            if (!this.IsRealClass || String.IsNullOrEmpty(actionParameter))
            {
                return;
            }

            var index = this.GetButton(actionParameter);

            if (index == -1)
            {
                return;
            }

            Remote.SetParameter($"{(this.IsStrip ? "Strip" : "Bus")}[{index + this.Offset}].{this.Command}", 0);

            this.AdjustmentValueChanged(actionParameter);
        }

        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            if (!this.IsRealClass || String.IsNullOrEmpty(actionParameter))
            {
                return;
            }

            var index = this.GetButton(actionParameter);

            if (index == -1 || this.Actions[index] is null)
            {
                return;
            }

            var newVal = this.Actions[index].Value + diff;
            if (newVal < this.MinValue)
            {
                newVal = this.MinValue;
            }
            else if (newVal > this.MaxValue)
            {
                newVal = this.MaxValue;
            }


            Remote.SetParameter($"{(this.IsStrip ? "Strip" : "Bus")}[{index + this.Offset}].{this.Command}",
                newVal / this.ScaleFactor);

            this.AdjustmentValueChanged(actionParameter);
        }

        protected override BitmapImage GetAdjustmentImage(String actionParameter, PluginImageSize imageSize)
        {
            if (!this.IsRealClass || String.IsNullOrEmpty(actionParameter))
            {
                return null;
            }

            var index = this.GetButton(actionParameter);

            if (this.Actions[index] is null || index == -1)
            {
                return base.GetAdjustmentImage(actionParameter, imageSize);
            }

            var (value, name, isMuted) = this.Actions[index];
            var backgroundColor = !isMuted ? this.Actions[index].Value > 0 ? ColorHelper.Danger : ColorHelper.Active : ColorHelper.Inactive;

            return DrawingHelper.DrawVolumeBar(imageSize, backgroundColor.ToBitmapColor(), BitmapColor.White, value, this.MinValue, this.MaxValue, this.ScaleFactor, name);
        }

        private Int32 GetButton(String actionParameter)
        {
            var number = actionParameter.Replace("VM-Strip", "").Replace($"-{this.Command}", "");

            if (Int32.TryParse(number, out var index))
            {
                return index;
            }
            else
            {
                return -1;
            }
        }

        private class AdjustmentItem
        {
            public Single Value { get; set; }
            public String Name { get; set; }
            public Boolean IsMuted { get; set; }

            public void Deconstruct(out Single value, out String name, out Boolean isMuted)
            {
                value = this.Value;
                name = this.Name;
                isMuted = this.IsMuted;
            }
        }
    }
}