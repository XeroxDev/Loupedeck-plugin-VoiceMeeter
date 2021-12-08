namespace Loupedeck.VoiceMeeterPlugin.Commands
{
    using Bases;

    using Helper;

    public class StripBCommand : BooleanBaseCommand
    {
        public StripBCommand() : base(true, true) =>
            this.CreateCommands(
                VoiceMeeterHelper.GetHardwareInputCount() + VoiceMeeterHelper.GetVirtualInputCount(),
                "B",
                VoiceMeeterHelper.GetStripBCount(),
                0
            ).ConfigureAwait(false);
    }
}