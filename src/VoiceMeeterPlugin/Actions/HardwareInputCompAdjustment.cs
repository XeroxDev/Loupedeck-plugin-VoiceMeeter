namespace Loupedeck.VoiceMeeterPlugin.Actions
{
    using Bases;

    using Helper;

    public class HardwareInputCompAdjustment : SingleBaseAdjustment
    {
        public HardwareInputCompAdjustment() : base(true, true, true) =>
            this.CreateCommands(
                VoiceMeeterHelper.GetHardwareInputCount(),
                "Comp",
                0
            ).ConfigureAwait(false);
    }
}