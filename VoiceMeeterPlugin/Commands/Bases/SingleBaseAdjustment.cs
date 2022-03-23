namespace Loupedeck.VoiceMeeterPlugin.Commands.Bases
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.IO;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading.Tasks;

    using Extensions;

    using Library.Voicemeeter;

    using Services;

    public class SingleBaseAdjustment : PluginDynamicAdjustment
    {
        private Single[] Actions { get; set; }
        private VoiceMeeterService VmService { get; }
        private String Command { get; set; }
        private Subject<Boolean> OnDestroy { get; } = new();
        private Int32 Offset { get; set; }
        private Boolean IsStrip { get; }
        private Int32 MaxValue { get; }
        private Int32 MinValue { get; }
        public Boolean IsRealClass { get; set; }
        protected Boolean Loaded { get; set; }

        public SingleBaseAdjustment(Boolean hasRestart, Boolean isRealClass, Boolean isStrip, Int32 minValue = 0,
            Int32 maxValue = 10) : base(hasRestart)
        {
            this.IsRealClass = isRealClass;
            this.IsStrip = isStrip;

            this.MinValue = minValue;
            this.MaxValue = maxValue < 0 ? maxValue * -1 : maxValue;

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

            this.Actions = new Single[stripCount];
            for (var hi = 0; hi < stripCount; hi++)
            {
                var name = Remote.GetTextParameter($"{(this.IsStrip ? "Strip" : "Bus")}[{hi + this.Offset}].Label");
                var groupName = String.IsNullOrEmpty(name) ? this.IsStrip ? "Strip" : "Bus" : name;
                this.AddParameter(
                    $"VM-Strip{hi}-{cmd}",
                    this.DisplayName,
                    $"{groupName} ({hi + 1 + this.Offset})",
                    "Input"
                );
            }

            this.GetNewSettings();
        }

        private void GetNewSettings()
        {
            for (var hiIndex = 0; hiIndex < this.Actions.Length; hiIndex++)
            {
                this.Actions[hiIndex] =
                    (Int32)Remote.GetParameter(
                        $"{(this.IsStrip ? "Strip" : "Bus")}[{hiIndex + this.Offset}].{this.Command}");
            }

            if (this.Loaded)
            {
                this.AdjustmentValueChanged();
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

            if (index == -1)
            {
                return;
            }
            var newVal = this.Actions[index] + diff;
            if (newVal < this.MinValue)
            {
                newVal = this.MinValue;
            }

            if (newVal > this.MaxValue)
            {
                newVal = this.MaxValue;
            }

            Remote.SetParameter($"{(this.IsStrip ? "Strip" : "Bus")}[{index + this.Offset}].{this.Command}",
                newVal);

            this.AdjustmentValueChanged(actionParameter);
        }

        protected override BitmapImage GetAdjustmentImage(String actionParameter, PluginImageSize imageSize)
        {
            if (!this.IsRealClass || String.IsNullOrEmpty(actionParameter))
            {
                return null;
            }

            var index = this.GetButton(actionParameter);

            if (index == -1)
            {
                return base.GetAdjustmentImage(actionParameter, imageSize);
            }

            var bitmap = new Bitmap(70, 20);
            var g = Graphics.FromImage(bitmap);

            var currentValue = this.Actions[index];
            var percentage = (currentValue - this.MinValue) / (this.MaxValue - this.MinValue) * 100;

            var bgColor = Color.FromArgb(156, 156, 156);
            var textColor = Color.White;
            var rect = new Rectangle(0, 0, bitmap.Width - 1, bitmap.Height - 1);
            var font = new Font("Arial", 20, FontStyle.Bold);
            var brush = new SolidBrush(Color.White);
            var width = (Int32)(rect.Width * percentage / 100.0);
            var sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

            g.DrawRectangle(new Pen(bgColor), rect);
            g.FillRectangle(new SolidBrush(bgColor), 0, 0, width, rect.Height);
            g.FillRectangle(new SolidBrush(Color.FromArgb(150, 0, 0, 0)), 0, 0, bitmap.Width, bitmap.Height);
            g.DrawAutoAdjustedFont(currentValue.ToString(CultureInfo.CurrentCulture), font, brush, rect, sf, 12);

            bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);

            var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            return BitmapImage.FromArray(ms.ToArray());
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
    }
}