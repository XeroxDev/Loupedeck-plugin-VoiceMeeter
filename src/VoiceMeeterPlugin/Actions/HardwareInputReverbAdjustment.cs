namespace Loupedeck.VoiceMeeterPlugin.Actions
{
    using Bases;

    using Helper;
    using Library.Voicemeeter;

    public class HardwareInputReverbAdjustment : SingleBaseAdjustment
    {
        public HardwareInputReverbAdjustment() : base(true, true, true)
        {
            if (Remote.Version != RunVoicemeeterParam.VoicemeeterPotato)
            {
                this.IsRealClass = false;
                return;
            }

            this.CreateCommands(
                VoiceMeeterHelper.GetHardwareInputCount(),
                "Reverb",
                0
            ).ConfigureAwait(false);
        }
    }
}