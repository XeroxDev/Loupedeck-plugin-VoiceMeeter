namespace Loupedeck.VoiceMeeterPlugin.Actions
{
    using Bases;

    using Helper;
    using Library.Voicemeeter;

    public class HardwareInputFx1Adjustment : SingleBaseAdjustment
    {
        public HardwareInputFx1Adjustment() : base(true, true, true)
        {
            if (Remote.Version != RunVoicemeeterParam.VoicemeeterPotato)
            {
                this.IsRealClass = false;
                return;
            }

            this.CreateCommands(
                VoiceMeeterHelper.GetHardwareInputCount(),
                "Fx1",
                0
            ).ConfigureAwait(false);
        }
    }

    public class HardwareInputFx2Adjustment : SingleBaseAdjustment
    {
        public HardwareInputFx2Adjustment() : base(true, true, true)
        {
            if (Remote.Version != RunVoicemeeterParam.VoicemeeterPotato)
            {
                this.IsRealClass = false;
                return;
            }

            this.CreateCommands(
                VoiceMeeterHelper.GetHardwareInputCount(),
                "Fx2",
                0
            ).ConfigureAwait(false);
        }
    }
}