namespace Loupedeck.VoiceMeeterPlugin.Commands
{
    using Bases;

    using Helper;

    public class StripACommand : BooleanBaseCommand
    {
        public StripACommand() : base(true, true) =>
            this.CreateCommands(
                VoiceMeeterHelper.GetHardwareInputCount() + VoiceMeeterHelper.GetVirtualInputCount(),
                "A",
                VoiceMeeterHelper.GetStripACount(),
                0
            ).ConfigureAwait(false);
    }
}