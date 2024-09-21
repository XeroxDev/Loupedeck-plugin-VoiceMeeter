namespace Loupedeck.VoiceMeeterPlugin.Actions
{
    using Bases;

    using Helpers;

    public class StripMuteCommand : BooleanBaseCommand
    {
        public StripMuteCommand() : base(true, true, ColorHelper.Danger) =>
            this.CreateCommands(
                VoiceMeeterHelper.GetHardwareInputCount() + VoiceMeeterHelper.GetVirtualInputCount(),
                "Mute",
                0
            ).ConfigureAwait(false);
    }
}