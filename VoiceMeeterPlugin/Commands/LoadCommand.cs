namespace Loupedeck.VoiceMeeterPlugin.Commands
{
    using System;
    using System.IO;

    using Helper;

    using Library.Voicemeeter;

    public class LoadCommand : PluginDynamicCommand
    {
        public LoadCommand() : base("Load", "Load settings", "Special")
        {
            this.MakeProfileAction("text;Path:");
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
            => DrawingHelper.DrawDefaultImage("Load", Path.GetFileNameWithoutExtension(actionParameter) ?? "", ColorHelper.Inactive);

        protected override void RunCommand(String actionParameter) => Remote.Load(actionParameter);
    }
}