namespace Loupedeck.VoiceMeeterPlugin.Commands
{
    using Bases;

    using Library.Voicemeeter;

    public class ShowCommand : SingleBaseCommand
    {
        public ShowCommand() : base("Show", "Show VoiceMeeter", "Special", Remote.Show)
        {
        }
    }
}