namespace Loupedeck.VoiceMeeterPlugin.Actions
{
    using Bases;

    using Helper;

    public class BusMuteCommand : BooleanBaseCommand
    {
        public BusMuteCommand() : base(true, false, ColorHelper.Danger) =>
            this.CreateCommands(
                VoiceMeeterHelper.GetHardwareInputCount() + VoiceMeeterHelper.GetVirtualInputCount(),
                "Mute",
                0
            ).ConfigureAwait(false);
    }
}