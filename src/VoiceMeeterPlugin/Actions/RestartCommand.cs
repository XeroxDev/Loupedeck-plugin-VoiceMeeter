namespace Loupedeck.VoiceMeeterPlugin.Actions
{
    using Bases;

    using Library.Voicemeeter;

    public class RestartCommand : SingleBaseCommand
    {
        public RestartCommand() : base("Restart", "Restart audio engine", "Special", Remote.Restart)
        {
        }
    }
}