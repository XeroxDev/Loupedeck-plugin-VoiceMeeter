namespace Loupedeck.VoiceMeeterPlugin.Services
{
    using System.Globalization;

    using Extensions;
    using Helpers;

    using Library.Voicemeeter;

    using SkiaSharp;

    public sealed class VoiceMeeterStateManager : IDisposable
    {
        public abstract class BindingDescriptor
        {
            public String StateKey { get; init; }
        }

        public sealed class BooleanBinding : BindingDescriptor
        {
            public Boolean IsStrip { get; init; }
            public Boolean IsMultiAction { get; init; }
            public String Command { get; init; }
            public Int32 Offset { get; init; }
            public Int32 MainIndex { get; init; }
            public Int32 ActionIndex { get; init; }
            public String DisplayName { get; init; }
            public SKColor ActiveColor { get; init; }
            public SKColor InactiveColor { get; init; }
            public String Api { get; init; }
        }

        public sealed class AdjustmentBinding : BindingDescriptor
        {
            public Boolean IsStrip { get; init; }
            public String Command { get; init; }
            public Int32 Offset { get; init; }
            public Int32 ChannelIndex { get; init; }
            public String DisplayName { get; init; }
            public Int32 MinValue { get; init; }
            public Int32 MaxValue { get; init; }
            public Int32 ScaleFactor { get; init; }
            public String Api { get; init; }
        }

        public sealed class RawCommandBinding : BindingDescriptor
        {
            public String Name { get; init; }
            public String Api { get; init; }
            public String Target { get; init; }
            public Boolean IsOn { get; init; }
            public SKColor OnColor { get; init; }
            public SKColor OffColor { get; init; }
        }

        public sealed class RawAdjustmentBinding : BindingDescriptor
        {
            public String Name { get; init; }
            public String Api { get; init; }
            public Single Steps { get; init; }
            public Int32 MinValue { get; init; }
            public Int32 MaxValue { get; init; }
            public SKColor BackgroundColor { get; init; }
            public SKColor ForegroundColor { get; init; }
        }

        public sealed class LevelBinding : BindingDescriptor
        {
            public String Name { get; init; }
            public LevelType LevelType { get; init; }
            public Int32 ChannelNumber { get; init; }
            public SKColor BackgroundColor { get; init; }
            public SKColor ForegroundColor { get; init; }
        }

        private sealed class CallbackRegistration : IDisposable
        {
            private readonly List<Action> _callbacks;
            private readonly Action _callback;

            public CallbackRegistration(List<Action> callbacks, Action callback)
            {
                this._callbacks = callbacks;
                this._callback = callback;
            }

            public void Dispose()
            {
                lock (this._callbacks)
                {
                    this._callbacks.Remove(this._callback);
                }
            }
        }

        public sealed class ChannelSnapshot
        {
            public String Key { get; set; }
            public String Parameter { get; set; }
            public String Label { get; set; }
            public Single Value { get; set; }
            public Boolean IsMuted { get; set; }
            public Boolean IsOn { get; set; }
            public BitmapImage Image { get; set; }
            internal readonly Dictionary<String, BitmapImage> RenderedImages = new(StringComparer.Ordinal);
        }

        public sealed class RawCommandSnapshot
        {
            public String Key { get; set; }
            public String Name { get; set; }
            public String Api { get; set; }
            public Boolean IsOn { get; set; }
            public SKColor OnColor { get; set; }
            public SKColor OffColor { get; set; }
            public BitmapImage Image { get; set; }
            internal readonly Dictionary<String, BitmapImage> RenderedImages = new(StringComparer.Ordinal);
        }

        public sealed class RawAdjustmentSnapshot
        {
            public String Key { get; set; }
            public String Name { get; set; }
            public String Api { get; set; }
            public Single Steps { get; set; }
            public Single Value { get; set; }
            public Int32 MinValue { get; set; }
            public Int32 MaxValue { get; set; }
            public SKColor BackgroundColor { get; set; }
            public SKColor ForegroundColor { get; set; }
            public BitmapImage Image { get; set; }
            internal readonly Dictionary<String, BitmapImage> RenderedImages = new(StringComparer.Ordinal);
        }

        public sealed class LevelSnapshot
        {
            public String Key { get; set; }
            public String Name { get; set; }
            public LevelType LevelType { get; set; }
            public Int32 ChannelNumber { get; set; }
            public Single Value { get; set; }
            public SKColor BackgroundColor { get; set; }
            public SKColor ForegroundColor { get; set; }
            public BitmapImage Image { get; set; }
            internal readonly Dictionary<String, BitmapImage> RenderedImages = new(StringComparer.Ordinal);
        }

        private sealed class BooleanDefinition
        {
            public Boolean IsStrip { get; init; }
            public Boolean IsMultiAction { get; init; }
            public String Command { get; init; }
            public Int32 Offset { get; init; }
            public Int32 MainIndex { get; init; }
            public Int32 ActionIndex { get; init; }
            public String DisplayName { get; init; }
            public SKColor ActiveColor { get; init; }
            public SKColor InactiveColor { get; init; }
        }

        private sealed class AdjustmentDefinition
        {
            public Boolean IsStrip { get; init; }
            public String Command { get; init; }
            public Int32 Offset { get; init; }
            public Int32 ChannelIndex { get; init; }
            public String DisplayName { get; init; }
            public Int32 MinValue { get; init; }
            public Int32 MaxValue { get; init; }
            public Int32 ScaleFactor { get; init; }
        }

        private readonly object _gate = new();
        private readonly object _connectionGate = new();
        private readonly SemaphoreSlim _reconnectSignal = new(0, 1);
        private readonly TimeSpan _startupDirtyGrace = TimeSpan.FromSeconds(5);
        private readonly Dictionary<String, List<Action>> _callbacks = new(StringComparer.Ordinal);
        private readonly Dictionary<String, BooleanDefinition> _booleanDefinitions = new(StringComparer.Ordinal);
        private readonly Dictionary<String, AdjustmentDefinition> _adjustmentDefinitions = new(StringComparer.Ordinal);
        private readonly Dictionary<String, ChannelSnapshot> _booleanSnapshots = new(StringComparer.Ordinal);
        private readonly Dictionary<String, ChannelSnapshot> _adjustmentSnapshots = new(StringComparer.Ordinal);
        private readonly Dictionary<String, RawCommandSnapshot> _rawCommandSnapshots = new(StringComparer.Ordinal);
        private readonly Dictionary<String, RawAdjustmentSnapshot> _rawAdjustmentSnapshots = new(StringComparer.Ordinal);
        private readonly Dictionary<String, LevelSnapshot> _levelSnapshots = new(StringComparer.Ordinal);
        private Parameters _parameters;
        private Levels _levels;
        private IDisposable _parametersSubscription;
        private IDisposable _levelsSubscription;
        private IDisposable _remoteSession;
        private ClientApplication _application;
        private CancellationTokenSource _reconnectCts;
        private Task _reconnectTask;
        private volatile Boolean _connected;
        private DateTime _connectedAtUtc;
        private DateTime _lastStartupDirtyLogUtc;
        private Int32 _consecutiveDirtyFailures;
        private Boolean _disposed;

        public Boolean Connected => this._connected;

        public event Action Changed;

        public async Task StartAsync(ClientApplication application)
        {
            PluginLog.Info("VM state manager: start requested");
            this.Stop();
            this._application = application;

            await this.TryConnectAsync().ConfigureAwait(false);

            this._reconnectCts = new CancellationTokenSource();
            this._reconnectTask = Task.Run(() => this.ReconnectLoopAsync(this._reconnectCts.Token));
            PluginLog.Info("VM state manager: reconnect loop started");
        }

        public void Stop()
        {
            PluginLog.Info("VM state manager: stop requested");
            this._reconnectCts?.Cancel();
            this._reconnectCts?.Dispose();
            this._reconnectCts = null;
            this._reconnectTask = null;

            this._application = null;
            this._connected = false;

            this.DisconnectSession(false);

            lock (this._gate)
            {
                this._callbacks.Clear();
                this._booleanDefinitions.Clear();
                this._adjustmentDefinitions.Clear();
                this._booleanSnapshots.Clear();
                this._adjustmentSnapshots.Clear();
                this._rawCommandSnapshots.Clear();
                this._rawAdjustmentSnapshots.Clear();
                this._levelSnapshots.Clear();
            }
        }

        private async Task<Boolean> TryConnectAsync()
        {
            if (this._disposed || this._connected)
            {
                return this._connected;
            }

            var application = this._application;
            if (application == null)
            {
                return false;
            }

            IDisposable remoteSession;
            try
            {
                PluginLog.Info("VM connect: initializing remote session");
                remoteSession = await Remote.Initialize(RunVoicemeeterParam.None, application).ConfigureAwait(false);
            }
            catch
            {
                PluginLog.Warning("VM connect: remote initialization threw");
                return false;
            }

            if (remoteSession == null)
            {
                PluginLog.Warning("VM connect: remote initialization returned null");
                return false;
            }

            lock (this._connectionGate)
            {
                if (this._disposed)
                {
                    remoteSession.Dispose();
                    return false;
                }

                if (this._connected)
                {
                    remoteSession.Dispose();
                    return true;
                }

                this._remoteSession?.Dispose();
                this._remoteSession = remoteSession;
                this._connected = true;
                this._connectedAtUtc = DateTime.UtcNow;
                this._lastStartupDirtyLogUtc = default;
                this._consecutiveDirtyFailures = 0;
                PluginLog.Info("VM connect: session established");

                this._parametersSubscription?.Dispose();
                this._parameters?.Dispose();
                this._parameters = new Parameters();
                this._parametersSubscription = this._parameters.Subscribe(response => this.OnParametersPolled(response));
                PluginLog.Verbose("VM connect: parameter polling subscribed");

                this._levelsSubscription?.Dispose();
                this._levels?.Dispose();
                this._levels = new Levels();
                this._levelsSubscription = this._levels.Subscribe(_ => this.RefreshFromLevels());
                PluginLog.Verbose("VM connect: level polling subscribed");
            }

            PluginLog.Info("VM sync: refresh-all start");
            this.RefreshAll();
            PluginLog.Info("VM sync: refresh-all end");
            return true;
        }

        private async Task ReconnectLoopAsync(CancellationToken cancellationToken)
        {
            var delay = TimeSpan.FromSeconds(1);
            var immediateRetry = false;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (this._connected)
                    {
                        PluginLog.Verbose("VM reconnect loop: waiting for reconnect signal");
                        await this._reconnectSignal.WaitAsync(cancellationToken).ConfigureAwait(false);
                        immediateRetry = true;
                        continue;
                    }

                    if (!immediateRetry)
                    {
                        var signaled = await this._reconnectSignal.WaitAsync((Int32)delay.TotalMilliseconds, cancellationToken).ConfigureAwait(false);
                        if (signaled)
                        {
                            PluginLog.Verbose("VM reconnect loop: signaled for immediate retry");
                            immediateRetry = true;
                            continue;
                        }
                    }

                    immediateRetry = false;

                    PluginLog.Info($"VM reconnect loop: trying connect, delay={delay.TotalSeconds:0}s");
                    if (await this.TryConnectAsync().ConfigureAwait(false))
                    {
                        delay = TimeSpan.FromSeconds(1);
                        PluginLog.Info("VM reconnect loop: connect succeeded");
                        continue;
                    }

                    delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 30));
                    PluginLog.Warning($"VM reconnect loop: connect failed, nextDelay={delay.TotalSeconds:0}s");
                }
            }
            catch (OperationCanceledException)
            {
                // Expected on shutdown.
            }
        }

        private void OnParametersPolled(Int32 response)
        {
            if (response < 0)
            {
                var connectedFor = DateTime.UtcNow - this._connectedAtUtc;
                var withinGrace = connectedFor < this._startupDirtyGrace;

                this._consecutiveDirtyFailures++;
                if (withinGrace)
                {
                    var shouldLog = this._lastStartupDirtyLogUtc == default || (DateTime.UtcNow - this._lastStartupDirtyLogUtc) >= TimeSpan.FromSeconds(5);
                    if (shouldLog)
                    {
                        this._lastStartupDirtyLogUtc = DateTime.UtcNow;
                        PluginLog.Verbose($"VM parameters: dirty poll returned {response} during startup grace ({connectedFor.TotalSeconds:0.0}s), ignoring");
                    }
                    return;
                }

                if (this._consecutiveDirtyFailures < 3)
                {
                    PluginLog.Verbose($"VM parameters: dirty poll returned {response}, consecutiveFailures={this._consecutiveDirtyFailures}, waiting for confirmation");
                    return;
                }

                PluginLog.Warning($"VM parameters: dirty poll returned {response}, disconnecting after {this._consecutiveDirtyFailures} consecutive failures");
                this.DisconnectSession();
                return;
            }

            if (this._consecutiveDirtyFailures > 0)
            {
                PluginLog.Verbose($"VM parameters: dirty poll recovered after {this._consecutiveDirtyFailures} consecutive failures");
            }

            this._consecutiveDirtyFailures = 0;
            this.RefreshFromParameters();
        }

        private void DisconnectSession(Boolean signalReconnect = true)
        {
            var wasConnected = false;

            lock (this._connectionGate)
            {
                wasConnected = this._connected;
                this._connected = false;
                this._connectedAtUtc = default;
                this._lastStartupDirtyLogUtc = default;
                this._consecutiveDirtyFailures = 0;

                this._parametersSubscription?.Dispose();
                this._parametersSubscription = null;

                this._levelsSubscription?.Dispose();
                this._levelsSubscription = null;

                this._parameters?.Dispose();
                this._parameters = null;

                this._levels?.Dispose();
                this._levels = null;

                this._remoteSession?.Dispose();
                this._remoteSession = null;
            }

            if (signalReconnect && wasConnected)
            {
                PluginLog.Warning("VM disconnect: session lost, signaling reconnect");
                this.SignalReconnect();
            }
            else if (wasConnected)
            {
                PluginLog.Info("VM disconnect: session stopped");
            }
        }

        private void SignalReconnect()
        {
            try
            {
                if (this._reconnectSignal.CurrentCount == 0)
                {
                    PluginLog.Verbose("VM reconnect: signal raised");
                    this._reconnectSignal.Release();
                }
            }
            catch
            {
                // ignored
            }
        }

        public IDisposable Subscribe(String key, Action callback)
        {
            var shouldInvokeImmediately = false;
            IDisposable registration;
            lock (this._gate)
            {
                if (!this._callbacks.TryGetValue(key, out var callbacks))
                {
                    callbacks = [];
                    this._callbacks[key] = callbacks;
                }

                if (!callbacks.Contains(callback))
                {
                    callbacks.Add(callback);
                }

                shouldInvokeImmediately = this.HasSnapshot(key);
                registration = new CallbackRegistration(callbacks, callback);
            }

            if (shouldInvokeImmediately)
            {
                callback();
            }

            return registration;
        }

        public IDisposable Subscribe(BindingDescriptor binding, Action callback) => binding == null ? null : this.Subscribe(binding.StateKey, callback);

        public static String BuildBooleanKey(String api) => $"boolean:{NormalizeKey(api)}";

        public static String BuildAdjustmentKey(String api) => $"adjustment:{NormalizeKey(api)}";

        public static String BuildLevelKey(String name, LevelType levelType, Int32 channelNumber, SKColor backgroundColor, SKColor foregroundColor) =>
            $"level:{BuildLevelConfig(name, backgroundColor, foregroundColor)}:{levelType}:{channelNumber}";

        public static String BuildRawCommandKey(String name, String api, SKColor onColor, SKColor offColor) =>
            $"raw-command:{BuildRawCommandConfig(name, onColor, offColor)}:{NormalizeRawCommandTarget(api)}";

        public static String BuildRawAdjustmentKey(String name, String api, Single steps, Int32 minValue, Int32 maxValue, SKColor backgroundColor, SKColor foregroundColor) =>
            $"raw-adjustment:{BuildRawAdjustmentConfig(name, steps, minValue, maxValue, backgroundColor, foregroundColor)}:{NormalizeKey(api)}";

        public BooleanBinding CreateBooleanBinding(Boolean isStrip, Boolean isMultiAction, String command, Int32 offset, Int32 mainIndex, Int32 actionIndex, String displayName, SKColor activeColor, SKColor inactiveColor)
        {
            var api = isMultiAction
                ? $"{(isStrip ? "Strip" : "Bus")}[{mainIndex + offset}].{command}{actionIndex + 1}"
                : $"{(isStrip ? "Strip" : "Bus")}[{actionIndex + offset}].{command}";

            return new BooleanBinding
            {
                StateKey = BuildBooleanKey(api),
                IsStrip = isStrip,
                IsMultiAction = isMultiAction,
                Command = command,
                Offset = offset,
                MainIndex = mainIndex,
                ActionIndex = actionIndex,
                DisplayName = displayName,
                ActiveColor = activeColor,
                InactiveColor = inactiveColor,
                Api = api,
            };
        }

        public AdjustmentBinding CreateAdjustmentBinding(Boolean isStrip, String command, Int32 offset, Int32 channelIndex, String displayName, Int32 minValue, Int32 maxValue, Int32 scaleFactor)
        {
            var api = $"{(isStrip ? "Strip" : "Bus")}[{channelIndex + offset}].{command}";
            return new AdjustmentBinding
            {
                StateKey = BuildAdjustmentKey(api),
                IsStrip = isStrip,
                Command = command,
                Offset = offset,
                ChannelIndex = channelIndex,
                DisplayName = displayName,
                MinValue = minValue,
                MaxValue = maxValue,
                ScaleFactor = scaleFactor,
                Api = api,
            };
        }

        public RawCommandBinding CreateRawCommandBinding(String name, String api, SKColor onColor, SKColor offColor)
        {
            var normalizedApi = NormalizeRawCommandTarget(api);
            return new RawCommandBinding
            {
                StateKey = BuildRawCommandKey(name, api, onColor, offColor),
                Name = name,
                Api = api,
                Target = normalizedApi,
                OnColor = onColor,
                OffColor = offColor,
            };
        }

        public RawAdjustmentBinding CreateRawAdjustmentBinding(String name, String api, Single steps, Int32 minValue, Int32 maxValue, SKColor backgroundColor, SKColor foregroundColor)
        {
            return new RawAdjustmentBinding
            {
                StateKey = BuildRawAdjustmentKey(name, api, steps, minValue, maxValue, backgroundColor, foregroundColor),
                Name = name,
                Api = api,
                Steps = steps,
                MinValue = minValue,
                MaxValue = maxValue,
                BackgroundColor = backgroundColor,
                ForegroundColor = foregroundColor,
            };
        }

        public LevelBinding CreateLevelBinding(String name, LevelType levelType, Int32 channelNumber, SKColor backgroundColor, SKColor foregroundColor)
        {
            return new LevelBinding
            {
                StateKey = BuildLevelKey(name, levelType, channelNumber, backgroundColor, foregroundColor),
                Name = name,
                LevelType = levelType,
                ChannelNumber = channelNumber,
                BackgroundColor = backgroundColor,
                ForegroundColor = foregroundColor,
            };
        }

        private Boolean HasSnapshot(String key) =>
            this._booleanSnapshots.ContainsKey(key)
            || this._adjustmentSnapshots.ContainsKey(key)
            || this._rawCommandSnapshots.ContainsKey(key)
            || this._rawAdjustmentSnapshots.ContainsKey(key)
            || this._levelSnapshots.ContainsKey(key);

        public void RegisterBooleanTarget(String key, Boolean isStrip, Boolean isMultiAction, String command, Int32 offset, Int32 mainIndex, Int32 actionIndex, String displayName, SKColor activeColor, SKColor inactiveColor)
        {
            lock (this._gate)
            {
                this._booleanDefinitions[key] = new BooleanDefinition
                {
                    IsStrip = isStrip,
                    IsMultiAction = isMultiAction,
                    Command = command,
                    Offset = offset,
                    MainIndex = mainIndex,
                    ActionIndex = actionIndex,
                    DisplayName = displayName,
                    ActiveColor = activeColor,
                    InactiveColor = inactiveColor,
                };
            }

            this.RefreshBooleanTarget(key);
        }

        public void RegisterAdjustmentTarget(String key, Boolean isStrip, String command, Int32 offset, Int32 channelIndex, String displayName, Int32 minValue, Int32 maxValue, Int32 scaleFactor)
        {
            lock (this._gate)
            {
                this._adjustmentDefinitions[key] = new AdjustmentDefinition
                {
                    IsStrip = isStrip,
                    Command = command,
                    Offset = offset,
                    ChannelIndex = channelIndex,
                    DisplayName = displayName,
                    MinValue = minValue,
                    MaxValue = maxValue,
                    ScaleFactor = scaleFactor,
                };
            }

            this.RefreshAdjustmentTarget(key);
        }

        public void RegisterRawCommandTarget(String key, String name, String api, SKColor onColor, SKColor offColor)
        {
            lock (this._gate)
            {
                this._rawCommandSnapshots[key] = new RawCommandSnapshot
                {
                    Key = key,
                    Name = name,
                    Api = api,
                    OnColor = onColor,
                    OffColor = offColor,
                };
            }

            this.RefreshRawCommandTarget(key);
        }

        public void RegisterRawAdjustmentTarget(String key, String name, String api, Single steps, Int32 minValue, Int32 maxValue, SKColor backgroundColor, SKColor foregroundColor)
        {
            lock (this._gate)
            {
                this._rawAdjustmentSnapshots[key] = new RawAdjustmentSnapshot
                {
                    Key = key,
                    Name = name,
                    Api = api,
                    Steps = steps,
                    MinValue = minValue,
                    MaxValue = maxValue,
                    BackgroundColor = backgroundColor,
                    ForegroundColor = foregroundColor,
                };
            }

            this.RefreshRawAdjustmentTarget(key);
        }

        public void RegisterLevelTarget(String key, String name, LevelType levelType, Int32 channelNumber, SKColor backgroundColor, SKColor foregroundColor)
        {
            lock (this._gate)
            {
                this._levelSnapshots[key] = new LevelSnapshot
                {
                    Key = key,
                    Name = name,
                    LevelType = levelType,
                    ChannelNumber = channelNumber,
                    BackgroundColor = backgroundColor,
                    ForegroundColor = foregroundColor,
                };
            }

            this.RefreshLevelTarget(key);
        }

        public void RegisterBooleanTarget(BooleanBinding binding) =>
            this.RegisterBooleanTarget(binding.StateKey, binding.IsStrip, binding.IsMultiAction, binding.Command, binding.Offset, binding.MainIndex, binding.ActionIndex, binding.DisplayName, binding.ActiveColor, binding.InactiveColor);

        public void RegisterAdjustmentTarget(AdjustmentBinding binding) =>
            this.RegisterAdjustmentTarget(binding.StateKey, binding.IsStrip, binding.Command, binding.Offset, binding.ChannelIndex, binding.DisplayName, binding.MinValue, binding.MaxValue, binding.ScaleFactor);

        public void RegisterRawCommandTarget(RawCommandBinding binding) =>
            this.RegisterRawCommandTarget(binding.StateKey, binding.Name, binding.Api, binding.OnColor, binding.OffColor);

        public void RegisterRawAdjustmentTarget(RawAdjustmentBinding binding) =>
            this.RegisterRawAdjustmentTarget(binding.StateKey, binding.Name, binding.Api, binding.Steps, binding.MinValue, binding.MaxValue, binding.BackgroundColor, binding.ForegroundColor);

        public void RegisterLevelTarget(LevelBinding binding) =>
            this.RegisterLevelTarget(binding.StateKey, binding.Name, binding.LevelType, binding.ChannelNumber, binding.BackgroundColor, binding.ForegroundColor);

        public ChannelSnapshot GetChannelSnapshot(String key)
        {
            lock (this._gate)
            {
                return this._booleanSnapshots.TryGetValue(key, out var booleanSnapshot)
                    ? booleanSnapshot
                : this._adjustmentSnapshots.TryGetValue(key, out var adjustmentSnapshot)
                        ? adjustmentSnapshot
                        : null;
            }
        }

        public ChannelSnapshot GetChannelSnapshot(BindingDescriptor binding) => binding == null ? null : this.GetChannelSnapshot(binding.StateKey);

        public String GetChannelLabel(Boolean isStrip, Int32 channelIndex)
        {
            lock (this._gate)
            {
                return this.ReadChannelLabel(isStrip, channelIndex);
            }
        }

        public BitmapImage GetCachedImage(String key)
        {
            lock (this._gate)
            {
                if (this._booleanSnapshots.TryGetValue(key, out var booleanSnapshot))
                {
                    return booleanSnapshot.Image;
                }

                if (this._adjustmentSnapshots.TryGetValue(key, out var adjustmentSnapshot))
                {
                    return adjustmentSnapshot.Image;
                }

                if (this._rawCommandSnapshots.TryGetValue(key, out var rawCommandSnapshot))
                {
                    return rawCommandSnapshot.Image;
                }

                if (this._rawAdjustmentSnapshots.TryGetValue(key, out var rawAdjustmentSnapshot))
                {
                    return rawAdjustmentSnapshot.Image;
                }

                if (this._levelSnapshots.TryGetValue(key, out var levelSnapshot))
                {
                    return levelSnapshot.Image;
                }
            }

            return null;
        }

        public String GetBooleanDisplayName(String key, Int32 state)
        {
            lock (this._gate)
            {
                if (!this._booleanSnapshots.TryGetValue(key, out var snapshot))
                {
                    return null;
                }

                return $"{this._booleanDefinitions[key].DisplayName}\n{(state == 1 ? "On" : "Off")}";
            }
        }

        public String GetBooleanDisplayName(BooleanBinding binding, Int32 state) => binding == null ? null : this.GetBooleanDisplayName(binding.StateKey, state);

        public BitmapImage GetBooleanImage(String key, Int32 state, String parameterName = null)
        {
            lock (this._gate)
            {
                if (!this._booleanSnapshots.TryGetValue(key, out var snapshot))
                {
                    return null;
                }

                var cacheKey = $"{state}:{parameterName ?? "default"}";
                if (snapshot.RenderedImages.TryGetValue(cacheKey, out var cachedImage))
                {
                    return cachedImage.Copy();
                }

                var definition = this._booleanDefinitions[key];
                var image = DrawingHelper.DrawDefaultImage(
                    definition.DisplayName,
                    snapshot.Label,
                    state == 1 ? definition.ActiveColor : definition.InactiveColor);

                var cachedCopy = image.Copy();
                snapshot.RenderedImages[cacheKey] = cachedCopy;
                snapshot.Image = cachedCopy;
                return cachedCopy.Copy();
            }
        }

        public BitmapImage GetBooleanImage(BooleanBinding binding, Int32 state, String parameterName = null) => binding == null ? null : this.GetBooleanImage(binding.StateKey, state, parameterName);

        public BitmapImage GetAdjustmentImage(String key, PluginImageSize imageSize)
        {
            lock (this._gate)
            {
                if (!this._adjustmentSnapshots.TryGetValue(key, out var snapshot))
                {
                    return null;
                }

                var cacheKey = imageSize.ToString();
                if (snapshot.RenderedImages.TryGetValue(cacheKey, out var image))
                {
                    return image;
                }

                var definition = this._adjustmentDefinitions[key];
                var backgroundColor = snapshot.IsMuted
                    ? ColorHelper.Inactive
                    : snapshot.Value > 0
                        ? ColorHelper.Danger
                        : definition.IsStrip
                            ? ColorHelper.Active
                            : ColorHelper.Inactive;

                image = DrawingHelper.DrawVolumeBar(
                    imageSize,
                    backgroundColor.ToBitmapColor(),
                    BitmapColor.White,
                    snapshot.Value,
                    definition.MinValue,
                    definition.MaxValue,
                    definition.ScaleFactor,
                    definition.Command,
                    snapshot.Label);

                snapshot.RenderedImages[cacheKey] = image;
                snapshot.Image = image;
                return image;
            }
        }

        public BitmapImage GetAdjustmentImage(AdjustmentBinding binding, PluginImageSize imageSize) => binding == null ? null : this.GetAdjustmentImage(binding.StateKey, imageSize);

        public BitmapImage GetRawCommandImage(String key)
        {
            lock (this._gate)
            {
                if (!this._rawCommandSnapshots.TryGetValue(key, out var snapshot))
                {
                    return null;
                }

                const String cacheKey = "default";
                if (snapshot.RenderedImages.TryGetValue(cacheKey, out var image))
                {
                    return image;
                }

                image = DrawingHelper.DrawDefaultImage(snapshot.Name, String.Empty, snapshot.IsOn ? snapshot.OnColor : snapshot.OffColor);
                snapshot.RenderedImages[cacheKey] = image;
                snapshot.Image = image;
                return image;
            }
        }

        public BitmapImage GetRawCommandImage(RawCommandBinding binding) => binding == null ? null : this.GetRawCommandImage(binding.StateKey);

        public BitmapImage GetRawAdjustmentImage(String key)
        {
            lock (this._gate)
            {
                if (!this._rawAdjustmentSnapshots.TryGetValue(key, out var snapshot))
                {
                    return null;
                }

                const String cacheKey = "default";
                if (snapshot.RenderedImages.TryGetValue(cacheKey, out var image))
                {
                    return image;
                }

                var rounded = (Single)Math.Round(snapshot.Value, this.GetDecimalPlaces(snapshot.Steps));
                image = DrawingHelper.DrawVolumeBar(
                    PluginImageSize.Width60,
                    snapshot.BackgroundColor.ToBitmapColor(),
                    snapshot.ForegroundColor.ToBitmapColor(),
                    rounded,
                    snapshot.MinValue,
                    snapshot.MaxValue,
                    1,
                    String.Empty,
                    snapshot.Name);

                snapshot.RenderedImages[cacheKey] = image;
                snapshot.Image = image;
                return image;
            }
        }

        public BitmapImage GetRawAdjustmentImage(RawAdjustmentBinding binding) => binding == null ? null : this.GetRawAdjustmentImage(binding.StateKey);

        public Single GetRawAdjustmentValue(String key)
        {
            lock (this._gate)
            {
                return this._rawAdjustmentSnapshots.TryGetValue(key, out var snapshot) ? snapshot.Value : 0;
            }
        }

        public Single GetRawAdjustmentValue(RawAdjustmentBinding binding) => binding == null ? 0 : this.GetRawAdjustmentValue(binding.StateKey);

        public Boolean GetRawCommandState(String key)
        {
            lock (this._gate)
            {
                return this._rawCommandSnapshots.TryGetValue(key, out var snapshot) && snapshot.IsOn;
            }
        }

        public Boolean GetRawCommandState(RawCommandBinding binding) => binding != null && this.GetRawCommandState(binding.StateKey);

        public LevelSnapshot GetLevelSnapshot(String key)
        {
            lock (this._gate)
            {
                return this._levelSnapshots.TryGetValue(key, out var snapshot) ? snapshot : null;
            }
        }

        public LevelSnapshot GetLevelSnapshot(LevelBinding binding) => binding == null ? null : this.GetLevelSnapshot(binding.StateKey);

        public String GetLevelDisplayName(String key)
        {
            lock (this._gate)
            {
                if (!this._levelSnapshots.TryGetValue(key, out var snapshot))
                {
                    return null;
                }

                var currentValue = (Single)Math.Round(snapshot.Value, 10);
                return $"{snapshot.Name} - {currentValue:P0}";
            }
        }

        public String GetLevelDisplayName(LevelBinding binding) => binding == null ? null : this.GetLevelDisplayName(binding.StateKey);

        public BitmapImage GetLevelImage(String key)
        {
            lock (this._gate)
            {
                if (!this._levelSnapshots.TryGetValue(key, out var snapshot))
                {
                    return null;
                }

                const String cacheKey = "default";
                if (snapshot.RenderedImages.TryGetValue(cacheKey, out var image))
                {
                    return image;
                }

                var clamped = Math.Clamp(snapshot.Value, 0, 1);
                image = DrawingHelper.DrawVolumeBar(
                    PluginImageSize.Width60,
                    snapshot.BackgroundColor.ToBitmapColor(),
                    snapshot.ForegroundColor.ToBitmapColor(),
                    clamped,
                    0,
                    1,
                    1,
                    String.Empty,
                    snapshot.Name,
                    false);

                snapshot.RenderedImages[cacheKey] = image;
                snapshot.Image = image;
                return image;
            }
        }

        public BitmapImage GetLevelImage(LevelBinding binding) => binding == null ? null : this.GetLevelImage(binding.StateKey);

        public void UpdateBooleanTargetState(String key, Boolean isOn)
        {
            lock (this._gate)
            {
                if (!this._booleanSnapshots.TryGetValue(key, out var snapshot))
                {
                    return;
                }

                if (snapshot.IsOn == isOn)
                {
                    return;
                }

                snapshot.IsOn = isOn;
                snapshot.RenderedImages.Clear();
                snapshot.Image = null;
            }

            this.NotifyChanged(key);
        }

        public void UpdateBooleanTargetState(BooleanBinding binding, Boolean isOn) => this.UpdateBooleanTargetState(binding?.StateKey, isOn);

        public void UpdateAdjustmentTargetState(String key, Single value, Boolean isMuted, String label = null)
        {
            lock (this._gate)
            {
                if (!this._adjustmentSnapshots.TryGetValue(key, out var snapshot))
                {
                    return;
                }

                if (snapshot.Value == value && snapshot.IsMuted == isMuted && snapshot.Label == label)
                {
                    return;
                }

                snapshot.Value = value;
                snapshot.IsMuted = isMuted;
                if (!String.IsNullOrEmpty(label))
                {
                    snapshot.Label = label;
                }

                snapshot.RenderedImages.Clear();
                snapshot.Image = null;
            }

            this.NotifyChanged(key);
        }

        public void UpdateAdjustmentTargetState(AdjustmentBinding binding, Single value, Boolean isMuted, String label = null) => this.UpdateAdjustmentTargetState(binding?.StateKey, value, isMuted, label);

        public void UpdateRawCommandState(String key, Boolean isOn)
        {
            lock (this._gate)
            {
                if (!this._rawCommandSnapshots.TryGetValue(key, out var snapshot))
                {
                    return;
                }

                if (snapshot.IsOn == isOn)
                {
                    return;
                }

                snapshot.IsOn = isOn;
                snapshot.RenderedImages.Clear();
                snapshot.Image = null;
            }

            this.NotifyChanged(key);
        }

        public void UpdateRawCommandState(RawCommandBinding binding, Boolean isOn) => this.UpdateRawCommandState(binding?.StateKey, isOn);

        public void UpdateRawAdjustmentState(String key, Single value)
        {
            lock (this._gate)
            {
                if (!this._rawAdjustmentSnapshots.TryGetValue(key, out var snapshot))
                {
                    return;
                }

                if (snapshot.Value == value)
                {
                    return;
                }

                snapshot.Value = value;
                snapshot.RenderedImages.Clear();
                snapshot.Image = null;
            }

            this.NotifyChanged(key);
        }

        public void UpdateRawAdjustmentState(RawAdjustmentBinding binding, Single value) => this.UpdateRawAdjustmentState(binding?.StateKey, value);

        private void RefreshAll()
        {
            PluginLog.Verbose("VM sync: refreshing parameters");
            this.RefreshFromParameters();
            PluginLog.Verbose("VM sync: refreshing levels");
            this.RefreshFromLevels();
        }

        private void RefreshFromParameters()
        {
            if (!this._connected)
            {
                return;
            }

            try
            {
                List<String> changedKeys = [];

                lock (this._gate)
                {
                    foreach (var key in this._booleanDefinitions.Keys.ToArray())
                    {
                        if (this.RefreshBooleanTarget(key, false))
                        {
                            changedKeys.Add(key);
                        }
                    }

                    foreach (var key in this._adjustmentDefinitions.Keys.ToArray())
                    {
                        if (this.RefreshAdjustmentTarget(key, false))
                        {
                            changedKeys.Add(key);
                        }
                    }

                    foreach (var key in this._rawCommandSnapshots.Keys.ToArray())
                    {
                        if (this.RefreshRawCommandTarget(key, false))
                        {
                            changedKeys.Add(key);
                        }
                    }

                    foreach (var key in this._rawAdjustmentSnapshots.Keys.ToArray())
                    {
                        if (this.RefreshRawAdjustmentTarget(key, false))
                        {
                            changedKeys.Add(key);
                        }
                    }
                }

                if (changedKeys.Count > 0)
                {
                    this.NotifyChanged(changedKeys);
                }
            }
            catch
            {
                this.DisconnectSession();
            }
        }

        private void RefreshFromLevels()
        {
            if (!this._connected)
            {
                return;
            }

            try
            {
                List<String> changedKeys = [];

                lock (this._gate)
                {
                    foreach (var key in this._levelSnapshots.Keys.ToArray())
                    {
                        if (this.RefreshLevelTarget(key, false))
                        {
                            changedKeys.Add(key);
                        }
                    }
                }

                if (changedKeys.Count > 0)
                {
                    this.NotifyChanged(changedKeys);
                }
            }
            catch
            {
                this.DisconnectSession();
            }
        }

        private Boolean RefreshBooleanTarget(String key, Boolean notify = true)
        {
            if (!this._connected)
            {
                return false;
            }

            if (!this._booleanDefinitions.TryGetValue(key, out var definition))
            {
                return false;
            }

            var snapshot = this._booleanSnapshots.TryGetValue(key, out var existing)
                ? existing
                : new ChannelSnapshot { Key = key };

            var parameter = this.GetBooleanParameter(definition);
            var value = this.TryGetBooleanState(parameter, out var isOn) && isOn;
            var label = this.ReadChannelLabel(definition.IsStrip, definition.IsMultiAction ? definition.MainIndex + definition.Offset : definition.ActionIndex + definition.Offset);

            if (snapshot.IsOn == value && snapshot.Label == label)
            {
                return false;
            }

            snapshot.Parameter = parameter;
            snapshot.IsOn = value;
            snapshot.Label = label;
            snapshot.Value = value ? 1 : 0;
            snapshot.IsMuted = false;
            snapshot.RenderedImages.Clear();
            snapshot.Image = null;
            this._booleanSnapshots[key] = snapshot;

            if (notify)
            {
                this.NotifyChanged(key);
            }

            return true;
        }

        private Boolean RefreshAdjustmentTarget(String key, Boolean notify = true)
        {
            if (!this._connected)
            {
                return false;
            }

            if (!this._adjustmentDefinitions.TryGetValue(key, out var definition))
            {
                return false;
            }

            var snapshot = this._adjustmentSnapshots.TryGetValue(key, out var existing)
                ? existing
                : new ChannelSnapshot { Key = key };

            var parameter = this.GetAdjustmentParameter(definition);
            var value = Remote.GetParameter($"{parameter}.{definition.Command}") * definition.ScaleFactor;
            var label = this.ReadChannelLabel(definition.IsStrip, definition.ChannelIndex + definition.Offset);
            var muted = Remote.GetParameter($"{parameter}.Mute") > 0;

            if (snapshot.Value == value && snapshot.Label == label && snapshot.IsMuted == muted)
            {
                return false;
            }

            snapshot.Parameter = parameter;
            snapshot.Value = value;
            snapshot.Label = label;
            snapshot.IsMuted = muted;
            snapshot.IsOn = value > 0;
            snapshot.RenderedImages.Clear();
            snapshot.Image = null;
            this._adjustmentSnapshots[key] = snapshot;

            if (notify)
            {
                this.NotifyChanged(key);
            }

            return true;
        }

        private Boolean RefreshRawCommandTarget(String key, Boolean notify = true)
        {
            if (!this._connected)
            {
                return false;
            }

            if (!this._rawCommandSnapshots.TryGetValue(key, out var snapshot))
            {
                return false;
            }

            var value = this.TryGetBooleanState(this.GetRawCommandTarget(snapshot.Api), out var isOn) && isOn;
            if (snapshot.IsOn == value)
            {
                return false;
            }

            snapshot.IsOn = value;
            snapshot.RenderedImages.Clear();
            snapshot.Image = null;

            if (notify)
            {
                this.NotifyChanged(key);
            }

            return true;
        }

        private Boolean RefreshRawAdjustmentTarget(String key, Boolean notify = true)
        {
            if (!this._connected)
            {
                return false;
            }

            if (!this._rawAdjustmentSnapshots.TryGetValue(key, out var snapshot))
            {
                return false;
            }

            var value = Remote.GetParameter(snapshot.Api);
            if (snapshot.Value == value)
            {
                return false;
            }

            snapshot.Value = value;
            snapshot.RenderedImages.Clear();
            snapshot.Image = null;

            if (notify)
            {
                this.NotifyChanged(key);
            }

            return true;
        }

        private Boolean RefreshLevelTarget(String key, Boolean notify = true)
        {
            if (!this._connected)
            {
                return false;
            }

            if (!this._levelSnapshots.TryGetValue(key, out var snapshot))
            {
                return false;
            }

            var value = Remote.GetLevel(snapshot.LevelType, snapshot.ChannelNumber);
            if (snapshot.Value == value)
            {
                return false;
            }

            snapshot.Value = value;
            snapshot.RenderedImages.Clear();
            snapshot.Image = null;

            if (notify)
            {
                this.NotifyChanged(key);
            }

            return true;
        }

        private void NotifyChanged(String key)
        {
            this.NotifyKeyChanged(key);
            this.Changed?.Invoke();
        }

        private void NotifyKeyChanged(String key)
        {
            Action[] callbacks;
            lock (this._gate)
            {
                callbacks = this._callbacks.TryGetValue(key, out var list) ? list.ToArray() : [];
            }

            foreach (var callback in callbacks)
            {
                callback();
            }
        }

        private void NotifyChanged(IEnumerable<String> keys)
        {
            var keyList = keys.Distinct(StringComparer.Ordinal).ToArray();
            foreach (var key in keyList)
            {
                this.NotifyKeyChanged(key);
            }

            if (keyList.Length > 0)
            {
                this.Changed?.Invoke();
            }
        }

        private String GetBooleanParameter(BooleanDefinition definition) =>
            definition.IsMultiAction
                ? $"{(definition.IsStrip ? "Strip" : "Bus")}[{definition.MainIndex + definition.Offset}].{definition.Command}{definition.ActionIndex + 1}"
                : $"{(definition.IsStrip ? "Strip" : "Bus")}[{definition.ActionIndex + definition.Offset}].{definition.Command}";

        private String GetAdjustmentParameter(AdjustmentDefinition definition) =>
            $"{(definition.IsStrip ? "Strip" : "Bus")}[{definition.ChannelIndex + definition.Offset}]";

        private String GetRawCommandTarget(String api)
        {
            if (String.IsNullOrWhiteSpace(api))
            {
                return null;
            }

            var commands = api.Split(new[] { ';', '\r', '\n', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (commands.Length == 0)
            {
                return null;
            }

            var first = commands[0].Trim();
            var index = first.IndexOf('=');
            return index >= 0 ? first[..index].Trim() : first;
        }

        private Boolean TryGetBooleanState(String api, out Boolean value)
        {
            value = false;

            if (String.IsNullOrWhiteSpace(api))
            {
                return false;
            }

            var probe = 0f;
            var result = RemoteWrapper.GetParameter(api, ref probe);
            if (result != 0)
            {
                return false;
            }

            value = (Int32)probe == 1;
            return true;
        }

        private String ReadChannelLabel(Boolean isStrip, Int32 channelIndex)
        {
            var parameter = $"{(isStrip ? "Strip" : "Bus")}[{channelIndex}]";
            var label = Remote.GetTextParameter($"{parameter}.Label");
            return String.IsNullOrEmpty(label) ? this.GetFallbackChannelName(isStrip, channelIndex) : label;
        }

        private String GetFallbackChannelName(Boolean isStrip, Int32 channelIndex)
        {
            return isStrip ? "Strip" : "Bus";
        }

        private static String BuildLevelConfig(String name, SKColor backgroundColor, SKColor foregroundColor) =>
            $"name={NormalizeKey(name)}|bg={ToKeyColor(backgroundColor)}|fg={ToKeyColor(foregroundColor)}";

        private static String BuildRawCommandConfig(String name, SKColor onColor, SKColor offColor) =>
            $"name={NormalizeKey(name)}|on={ToKeyColor(onColor)}|off={ToKeyColor(offColor)}";

        private static String BuildRawAdjustmentConfig(String name, Single steps, Int32 minValue, Int32 maxValue, SKColor backgroundColor, SKColor foregroundColor) =>
            $"name={NormalizeKey(name)}|steps={steps.ToString(CultureInfo.InvariantCulture)}|min={minValue}|max={maxValue}|bg={ToKeyColor(backgroundColor)}|fg={ToKeyColor(foregroundColor)}";

        private static String NormalizeRawCommandTarget(String api)
        {
            if (String.IsNullOrWhiteSpace(api))
            {
                return String.Empty;
            }

            var commands = api.Split(new[] { ';', '\r', '\n', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (commands.Length == 0)
            {
                return String.Empty;
            }

            var first = commands[0].Trim();
            var index = first.IndexOf('=');
            return index >= 0 ? first[..index].Trim() : first;
        }

        private static String NormalizeKey(String value) => String.IsNullOrWhiteSpace(value) ? String.Empty : value.Trim();

        private static String ToKeyColor(SKColor color) => $"#{color.Red:x2}{color.Green:x2}{color.Blue:x2}";

        private Int32 GetDecimalPlaces(Single value)
        {
            var str = value.ToString(CultureInfo.InvariantCulture);
            var index = str.IndexOf('.');
            return index == -1 ? 0 : str.Length - index - 1;
        }

        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this._disposed = true;
            this.Stop();
        }
    }
}
