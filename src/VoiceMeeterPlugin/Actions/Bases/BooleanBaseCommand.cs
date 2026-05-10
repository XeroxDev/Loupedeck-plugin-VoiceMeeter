namespace Loupedeck.VoiceMeeterPlugin.Actions.Bases
{
    using Extensions;

    using Helpers;

    using Library.Voicemeeter;

    using Services;

    using SkiaSharp;

    public abstract class BooleanBaseCommand : PluginMultistateDynamicCommand
    {
        private enum VMStates
        {
            Off,
            On
        }

        private readonly List<String> _actionParameters = [];
        private readonly List<IDisposable> _subscriptions = [];
        private readonly Dictionary<String, VoiceMeeterStateManager.BooleanBinding> _bindings = new(StringComparer.Ordinal);
        public Boolean IsRealClass { get; set; }
        private Boolean IsStrip { get; }
        private Int32 Offset { get; set; }
        private SKColor ActiveColor { get; }
        private SKColor InactiveColor { get; }
        private VoiceMeeterService VmService { get; }

        public BooleanBaseCommand(Boolean isRealClass, Boolean isStrip, SKColor? activeColor = null, SKColor? inactiveColor = null)
        {
            base.IsWidget = true;
            foreach (var state in Enum.GetValues(typeof(VMStates)))
            {
                this.AddState(state.ToString(), $"If the action is {state}");
            }

            this.IsRealClass = isRealClass;
            this.IsStrip = isStrip;
            this.ActiveColor = activeColor ?? ColorHelper.Active;
            this.InactiveColor = inactiveColor ?? ColorHelper.Inactive;
            if (!this.IsRealClass)
            {
                return;
            }

            this.VmService = VoiceMeeterService.Instance;
        }

        public async Task CreateCommands(Int32 stripCount, String cmd, Int32 specialCount, Int32 offset,
            String displayName = null)
        {
            this.Offset = offset;
            this.DisplayName = displayName ?? cmd;

            while (!this.VmService.Connected)
            {
                await Task.Delay(1000).ConfigureAwait(false);
            }

            for (var hi = 0; hi < stripCount; hi++)
            {
                for (var special = 1; special <= specialCount; special++)
                {
                    var key = GetActionParameterName(hi, cmd, special);
                    var binding = this.VmService.StateManager.CreateBooleanBinding(this.IsStrip, true, cmd, offset, hi, special - 1, $"{this.DisplayName}{special}", this.ActiveColor, this.InactiveColor);
                    this._actionParameters.Add(key);
                    this._bindings[key] = binding;

                    var groupName = this.VmService.StateManager.GetChannelLabel(this.IsStrip, hi + this.Offset);
                    this.VmService.StateManager.RegisterBooleanTarget(binding);

                    this.AddParameter(
                        key,
                        $"{this.DisplayName}{special}",
                        $"{groupName} ({hi + 1 + this.Offset})"
                    );
                }
            }
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
                var key = GetActionParameterName(hi, cmd);
                var binding = this.VmService.StateManager.CreateBooleanBinding(this.IsStrip, false, cmd, offset, 0, hi, this.DisplayName, this.ActiveColor, this.InactiveColor);
                this._actionParameters.Add(key);
                this._bindings[key] = binding;

                var groupName = this.VmService.StateManager.GetChannelLabel(this.IsStrip, hi + this.Offset);

                this.VmService.StateManager.RegisterBooleanTarget(binding);

                this.AddParameter(
                    key,
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
                    this.VmService.StateManager.Subscribe(binding, () =>
                    {
                        var snapshot = this.VmService.StateManager.GetChannelSnapshot(binding);
                        if (snapshot != null)
                        {
                            this.SetCurrentState(actionParameter, snapshot.IsOn ? VMStates.On.ToInt() : VMStates.Off.ToInt());
                        }

                        this.ActionImageChanged(actionParameter);
                    })
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
            if (!this.IsRealClass)
            {
                return;
            }

            if (!this._bindings.TryGetValue(actionParameter, out var binding))
            {
                return;
            }

            var snapshot = this.VmService.StateManager.GetChannelSnapshot(binding);
            var newState = snapshot is null || !snapshot.IsOn;
            Remote.SetParameter(binding.Api, newState ? 1 : 0);
            this.VmService.StateManager.UpdateBooleanTargetState(binding, newState);
            this.SetCurrentState(actionParameter, newState ? VMStates.On.ToInt() : VMStates.Off.ToInt());
            this.ActionImageChanged(actionParameter);
        }

        protected override String GetCommandDisplayName(String actionParameter, Int32 state, PluginImageSize imageSize)
        {
            if (!this.IsRealClass || actionParameter == null)
            {
                return null;
            }

            return this.VmService.StateManager.GetBooleanDisplayName(this._bindings[actionParameter], state);
        }

        protected override BitmapImage GetCommandImage(String actionParameter, Int32 state, PluginImageSize imageSize)
        {
            if (!this.IsRealClass)
            {
                return null;
            }

            if (!this._bindings.TryGetValue(actionParameter, out var binding))
            {
                return null;
            }

            return this.VmService.StateManager.GetBooleanImage(binding, state);
        }

        private static String GetActionParameterName(Int32 stripNumber, String cmd, Int32? specialNumber = null) =>
            specialNumber == null ? $"VM-Strip{stripNumber}-{cmd}2147483647" : $"VM-Strip{stripNumber}-{cmd}{specialNumber.Value}";

    }
}
