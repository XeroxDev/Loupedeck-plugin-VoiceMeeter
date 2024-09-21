namespace Loupedeck.VoiceMeeterPlugin.Actions
{
    using Bases;

    using Helpers;

    public class HardwareInputGateAdjustment : SingleBaseAdjustment
    {
        public HardwareInputGateAdjustment() : base(true, true, true) =>
            this.CreateCommands(
                VoiceMeeterHelper.GetHardwareInputCount(),
                "Gate",
                0
            ).ConfigureAwait(false);
    }
}