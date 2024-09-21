namespace Loupedeck.VoiceMeeterPlugin.Actions
{
    using Bases;

    using Helpers;

    public class HardwareInputMonoCommand : BooleanBaseCommand
    {
        public HardwareInputMonoCommand() : base(true, true) =>
            this.CreateCommands(
                VoiceMeeterHelper.GetHardwareInputCount(),
                "Mono",
                0
            ).ConfigureAwait(false);
    }
}