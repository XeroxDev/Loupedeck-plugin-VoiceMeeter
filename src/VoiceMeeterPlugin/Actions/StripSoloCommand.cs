namespace Loupedeck.VoiceMeeterPlugin.Actions
{
    using Bases;

    using Helper;

    public class StripSoloCommand : BooleanBaseCommand
    {
        public StripSoloCommand() : base(true, true) =>
            this.CreateCommands(
                VoiceMeeterHelper.GetHardwareInputCount() + VoiceMeeterHelper.GetVirtualInputCount(),
                "Solo",
                0
            ).ConfigureAwait(false);
    }
}