namespace Loupedeck.VoiceMeeterPlugin.Services
{
    using System;
    using System.Threading.Tasks;

    using Library.Voicemeeter;

    public sealed class VoiceMeeterService
    {
        public static VoiceMeeterService Instance => Lazy.Value;

        private static readonly Lazy<VoiceMeeterService> Lazy = new(() => new VoiceMeeterService());

        public Parameters Parameters { get; set; }
        public Boolean Connected { get; set; }

        public async Task StartService(ClientApplication application)
        {
            await Remote.Initialize(RunVoicemeeterParam.None, application);

            this.Connected = true;
            this.Parameters = new Parameters();
        }
    }
}