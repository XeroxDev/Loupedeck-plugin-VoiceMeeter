namespace Loupedeck.VoiceMeeterPlugin.Services
{
    public sealed class VoiceMeeterService
    {
        public static VoiceMeeterService Instance => Lazy.Value;

        private static readonly Lazy<VoiceMeeterService> Lazy = new(() => new VoiceMeeterService());

        public VoiceMeeterStateManager StateManager { get; } = new();
        public Boolean Connected => this.StateManager.Connected;

        public Task StartService(ClientApplication application) =>
            this.StateManager.StartAsync(application);

        public void StopService() => this.StateManager.Stop();
    }
}
