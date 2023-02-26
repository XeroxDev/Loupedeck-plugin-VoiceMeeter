namespace Loupedeck.VoiceMeeterPlugin.Actions
{
    using Bases;

    using Library.Voicemeeter;

    public class EjectCommand : SingleBaseCommand
    {
        public EjectCommand() : base("Eject", "Eject Cassette", "Special", Remote.Eject)
        {
        }
    }
}