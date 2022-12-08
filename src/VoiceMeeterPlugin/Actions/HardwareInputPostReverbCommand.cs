namespace Loupedeck.VoiceMeeterPlugin.Actions
{
    using Bases;

    using Helper;
    using Library.Voicemeeter;

    public class HardwareInputPostReverbCommand : BooleanBaseCommand
    {
        public HardwareInputPostReverbCommand() : base(true, true)
        {
            if (Remote.Version != RunVoicemeeterParam.VoicemeeterPotato)
            {
                this.IsRealClass = false;
                return;
            }

            this.CreateCommands(
                VoiceMeeterHelper.GetHardwareInputCount(),
                "PostReverb",
                0
            ).ConfigureAwait(false);
        }
    }
}