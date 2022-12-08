namespace Loupedeck.VoiceMeeterPlugin.Actions
{
    using Bases;

    using Helper;
    using Library.Voicemeeter;

    public class HardwareInputPostDelayCommand : BooleanBaseCommand
    {
        public HardwareInputPostDelayCommand() : base(true, true)
        {
            if (Remote.Version != RunVoicemeeterParam.VoicemeeterPotato)
            {
                this.IsRealClass = false;
                return;
            }

            this.CreateCommands(
                VoiceMeeterHelper.GetHardwareInputCount(),
                "PostDelay",
                0
            ).ConfigureAwait(false);
        }
    }
}