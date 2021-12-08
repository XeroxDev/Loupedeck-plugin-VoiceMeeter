namespace Loupedeck.VoiceMeeterPlugin.Commands
{
    using Bases;

    using Helper;

    public class BusMuteCommand : BooleanBaseCommand
    {
        public BusMuteCommand() : base(true, false) =>
            this.CreateCommands(
                VoiceMeeterHelper.GetHardwareInputCount() + VoiceMeeterHelper.GetVirtualInputCount(),
                "Mute",
                0
            ).ConfigureAwait(false);
    }
}