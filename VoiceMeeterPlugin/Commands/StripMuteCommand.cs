namespace Loupedeck.VoiceMeeterPlugin.Commands
{
    using Bases;

    using Helper;

    public class StripMuteCommand : BooleanBaseCommand
    {
        public StripMuteCommand() : base(true, true) =>
            this.CreateCommands(
                VoiceMeeterHelper.GetHardwareInputCount() + VoiceMeeterHelper.GetVirtualInputCount(),
                "Mute",
                0
            ).ConfigureAwait(false);
    }
}