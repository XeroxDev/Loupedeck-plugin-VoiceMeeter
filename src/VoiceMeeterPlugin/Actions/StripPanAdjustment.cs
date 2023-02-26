namespace Loupedeck.VoiceMeeterPlugin.Actions
{
    using Bases;

    using Helper;

    public class StripPanXAdjustment : SingleBaseAdjustment
    {
        public StripPanXAdjustment() : base(true, true, true, -5, 5, 10) =>
            this.CreateCommands(
                VoiceMeeterHelper.GetHardwareInputCount() + VoiceMeeterHelper.GetVirtualInputCount(),
                "Pan_x",
                0
            ).ConfigureAwait(false);
    }

    public class HardwareInputPanYAdjustment : SingleBaseAdjustment
    {
        public HardwareInputPanYAdjustment() : base(true, true, true, 0, 10, 10) =>
            this.CreateCommands(
                VoiceMeeterHelper.GetHardwareInputCount(),
                "Pan_y",
                0
            ).ConfigureAwait(false);
    }

    public class VirtualInputPanYAdjustment : SingleBaseAdjustment
    {
        public VirtualInputPanYAdjustment() : base(true, true, true, -5, 5, 10) =>
            this.CreateCommands(
                VoiceMeeterHelper.GetVirtualInputCount(),
                "Pan_y",
                VoiceMeeterHelper.GetHardwareInputCount()
            ).ConfigureAwait(false);
    }
}