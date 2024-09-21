namespace Loupedeck.VoiceMeeterPlugin.Actions
{
    using Bases;

    using Helpers;

    public class StripGainAdjustment : SingleBaseAdjustment
    {
        public StripGainAdjustment() : base(true, true, true, -60, 12) =>
            this.CreateCommands(
                VoiceMeeterHelper.GetHardwareInputCount() + VoiceMeeterHelper.GetVirtualInputCount(),
                "Gain",
                0
            ).ConfigureAwait(false);
    }
}