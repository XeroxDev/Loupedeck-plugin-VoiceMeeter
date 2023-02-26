namespace Loupedeck.VoiceMeeterPlugin.Actions
{
    using Bases;

    using Library.Voicemeeter;

    public class ResetCommand : SingleBaseCommand
    {
        public ResetCommand() : base("Reset", "Reset ALL configuration of VoiceMeeter", "Special", Remote.Reset)
        {
        }
    }
}