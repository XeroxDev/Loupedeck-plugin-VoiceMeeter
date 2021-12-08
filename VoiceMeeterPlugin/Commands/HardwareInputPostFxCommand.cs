namespace Loupedeck.VoiceMeeterPlugin.Commands
{
    using Bases;

    using Helper;

    using Library.Voicemeeter;

    public class HardwareInputPostFx1Command : BooleanBaseCommand
    {
        public HardwareInputPostFx1Command() : base(true, true)
        {
            if (Remote.Version != RunVoicemeeterParam.VoicemeeterPotato)
            {
                this.IsRealClass = false;
                return;
            }

            this.CreateCommands(
                VoiceMeeterHelper.GetHardwareInputCount(),
                "PostFx1",
                0
            ).ConfigureAwait(false);
        }
    }

    public class HardwareInputPostFx2Command : BooleanBaseCommand
    {
        public HardwareInputPostFx2Command() : base(true, true)
        {
            if (Remote.Version != RunVoicemeeterParam.VoicemeeterPotato)
            {
                this.IsRealClass = false;
                return;
            }

            this.CreateCommands(
                VoiceMeeterHelper.GetHardwareInputCount(),
                "PostFx2",
                0
            ).ConfigureAwait(false);
        }
    }
}