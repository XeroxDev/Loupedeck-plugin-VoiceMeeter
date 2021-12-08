namespace Loupedeck.VoiceMeeterPlugin.Commands
{
    using Bases;

    using Library.Voicemeeter;

    public class EjectCommand : SingleBaseCommand
    {
        public EjectCommand() : base("Eject","Eject Cassette", "Special", Remote.Eject)
        {
        }
    }
}