namespace Loupedeck.VoiceMeeterPlugin.Commands
{
    using Bases;

    using Library.Voicemeeter;

    public class ShutdownCommand : SingleBaseCommand
    {
        public ShutdownCommand() : base("Shutdown", "Shutdown VoiceMeeter", "Special", Remote.Shutdown)
        {
        }
    }
}