namespace Loupedeck.VoiceMeeterPlugin.Actions
{
    using Bases;

    using Helpers;

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