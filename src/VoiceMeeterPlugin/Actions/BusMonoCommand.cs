namespace Loupedeck.VoiceMeeterPlugin.Actions
{
    using Bases;

    using Helper;

    public class BusMonoCommand : BooleanBaseCommand
    {
        public BusMonoCommand() : base(true, false) =>
            this.CreateCommands(
                VoiceMeeterHelper.GetHardwareInputCount() + VoiceMeeterHelper.GetVirtualInputCount(),
                "Mono",
                0
            ).ConfigureAwait(false);
    }
}