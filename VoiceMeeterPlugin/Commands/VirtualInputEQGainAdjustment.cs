namespace Loupedeck.VoiceMeeterPlugin.Commands
{
    using Bases;

    using Helper;

    using Library.Voicemeeter;

    public class VirtualInputEqGain1Adjustment : SingleBaseAdjustment
    {
        public VirtualInputEqGain1Adjustment() : base(true, true, true, -12, 12) =>
            this.CreateCommands(
                VoiceMeeterHelper.GetVirtualInputCount(),
                "EQGain1",
                VoiceMeeterHelper.GetHardwareInputCount()
            ).ConfigureAwait(false);
    }

    public class VirtualInputEqGain2Adjustment : SingleBaseAdjustment
    {
        public VirtualInputEqGain2Adjustment() : base(true, true, true, -12, 12) =>
            this.CreateCommands(
                VoiceMeeterHelper.GetVirtualInputCount(),
                "EQGain2",
                VoiceMeeterHelper.GetHardwareInputCount()
            ).ConfigureAwait(false);
    }

    public class VirtualInputEqGain3Adjustment : SingleBaseAdjustment
    {
        public VirtualInputEqGain3Adjustment() : base(true, true, true, -12, 12)
        {
            if (Remote.Version != RunVoicemeeterParam.VoicemeeterPotato)
            {
                this.IsRealClass = false;
                return;
            }

            this.CreateCommands(
                VoiceMeeterHelper.GetVirtualInputCount(),
                "EQGain3",
                VoiceMeeterHelper.GetHardwareInputCount()
            ).ConfigureAwait(false);
        }
    }
}