namespace Loupedeck.VoiceMeeterPlugin.Actions
{
    using Bases;

    using Helper;
    using Library.Voicemeeter;

    public class HardwareInputDelayAdjustment : SingleBaseAdjustment
    {
        public HardwareInputDelayAdjustment() : base(true, true, true)
        {
            if (Remote.Version != RunVoicemeeterParam.VoicemeeterPotato)
            {
                this.IsRealClass = false;
                return;
            }

            this.CreateCommands(
                VoiceMeeterHelper.GetHardwareInputCount(),
                "Delay",
                0
            ).ConfigureAwait(false);
        }
    }
}