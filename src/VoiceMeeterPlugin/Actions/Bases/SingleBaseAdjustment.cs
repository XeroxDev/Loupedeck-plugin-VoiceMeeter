namespace Loupedeck.VoiceMeeterPlugin.Actions.Bases
{
    using Helpers;

    using Library.Voicemeeter;

    using Services;

    public abstract class SingleBaseAdjustment : PluginDynamicAdjustment
    {
        private readonly List<String> _actionParameters = [];
        private readonly List<IDisposable> _subscriptions = [];
        private readonly Dictionary<String, VoiceMeeterStateManager.AdjustmentBinding> _bindings = new(StringComparer.Ordinal);
        private VoiceMeeterService VmService { get; }
        private Int32 Offset { get; set; }
        private Boolean IsStrip { get; }
        private Int32 MaxValue { get; }
        private Int32 MinValue { get; }
        private Int32 ScaleFactor { get; }
        public Boolean IsRealClass { get; set; }

        public SingleBaseAdjustment(Boolean hasRestart, Boolean isRealClass, Boolean isStrip, Int32 minValue = 0,
            Int32 maxValue = 10, Int32 scaleFactor = 1) : base(hasRestart)
        {
            base.IsWidget = true;
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
            this.Offset = offset;
            this.DisplayName = displayName ?? cmd;

            while (!this.VmService.Connected)
            {
                await Task.Delay(1000).ConfigureAwait(false);
            }

            for (var hi = 0; hi < stripCount; hi++)
            {
                var parameterKey = GetActionParameterName(hi, cmd);
                var binding = this.VmService.StateManager.CreateAdjustmentBinding(this.IsStrip, cmd, this.Offset, hi, this.DisplayName, this.MinValue, this.MaxValue, this.ScaleFactor);
                this._actionParameters.Add(parameterKey);
                this._bindings[parameterKey] = binding;

                var groupName = this.VmService.StateManager.GetChannelLabel(this.IsStrip, hi + this.Offset);

                this.VmService.StateManager.RegisterAdjustmentTarget(binding);

                this.AddParameter(
                    parameterKey,
                    this.DisplayName,
                    $"{groupName} ({hi + 1 + this.Offset})"
                );
            }
        }

        protected override Boolean OnLoad()
        {
            if (!this.IsRealClass)
            {
                return base.OnLoad();
            }

            foreach (var actionParameter in this._actionParameters)
            {
                var binding = this._bindings[actionParameter];
                this._subscriptions.Add(
                    this.VmService.StateManager.Subscribe(binding, () => this.AdjustmentValueChanged(actionParameter))
                );
            }

            return base.OnLoad();
        }

        protected override Boolean OnUnload()
        {
            if (!this.IsRealClass)
            {
                return base.OnUnload();
            }

            foreach (var subscription in this._subscriptions)
            {
                subscription.Dispose();
            }

            this._subscriptions.Clear();
            return base.OnUnload();
        }

        protected override void RunCommand(String actionParameter)
        {
            if (!this.IsRealClass || String.IsNullOrEmpty(actionParameter))
            {
                return;
            }

            if (!this._bindings.TryGetValue(actionParameter, out var binding))
            {
                return;
            }

            var snapshot = this.VmService.StateManager.GetChannelSnapshot(binding);
            if (snapshot is null)
            {
                return;
            }

            Remote.SetParameter(binding.Api, 0);
            this.VmService.StateManager.UpdateAdjustmentTargetState(binding, 0, snapshot.IsMuted, snapshot.Label);
            this.AdjustmentValueChanged(actionParameter);
        }

        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            if (!this.IsRealClass || String.IsNullOrEmpty(actionParameter))
            {
                return;
            }

            if (!this._bindings.TryGetValue(actionParameter, out var binding))
            {
                return;
            }

            var snapshot = this.VmService.StateManager.GetChannelSnapshot(binding);
            if (snapshot is null)
            {
                return;
            }

            var newVal = (Int32)snapshot.Value + diff;
            if (newVal < this.MinValue)
            {
                newVal = this.MinValue;
            }
            else if (newVal > this.MaxValue)
            {
                newVal = this.MaxValue;
            }

            var normalizedValue = (Single)newVal / this.ScaleFactor;
            Remote.SetParameter(binding.Api, normalizedValue);
            this.VmService.StateManager.UpdateAdjustmentTargetState(binding, normalizedValue, snapshot.IsMuted, snapshot.Label);
            this.AdjustmentValueChanged(actionParameter);
        }

        protected override BitmapImage GetAdjustmentImage(String actionParameter, PluginImageSize imageSize)
        {
            if (!this.IsRealClass || String.IsNullOrEmpty(actionParameter))
            {
                return null;
            }

            return this._bindings.TryGetValue(actionParameter, out var binding)
                ? this.VmService.StateManager.GetAdjustmentImage(binding, imageSize)
                : null;
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize) => this.GetAdjustmentImage(actionParameter, imageSize);

        protected override Double? GetAdjustmentMinValue(String actionParameter) => this.MinValue;

        protected override Double? GetAdjustmentMaxValue(String actionParameter) => this.MaxValue;

        private static String GetActionParameterName(Int32 stripNumber, String cmd) => $"VM-Strip{stripNumber}-{cmd}";

    }
}
