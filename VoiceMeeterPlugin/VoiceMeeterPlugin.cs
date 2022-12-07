namespace Loupedeck.VoiceMeeterPlugin
{
    using System;

    using Helper;

    using Library.Voicemeeter;

    public class VoiceMeeterPlugin : Plugin
    {
        public override Boolean UsesApplicationApiOnly => true;

        public override void Load()
        {
            this.Info.Icon16x16 = DrawingHelper.ReadImage("icon-16", addPath: "Icon");
            this.Info.Icon32x32 = DrawingHelper.ReadImage("icon-32", addPath: "Icon");
            this.Info.Icon48x48 = DrawingHelper.ReadImage("icon-48", addPath: "Icon");
            this.Info.Icon256x256 = DrawingHelper.ReadImage("icon-256", addPath: "Icon");
        }

        public override void Unload()
        {
            try
            {
                RemoteWrapper.Logout();
            }
            catch
            {
                // ignored
            }
        }

        public override void RunCommand(String commandName, String parameter)
        {
        }

        public override void ApplyAdjustment(String adjustmentName, String parameter, Int32 diff)
        {
        }
    }
}