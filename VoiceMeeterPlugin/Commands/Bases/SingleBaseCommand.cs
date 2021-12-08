namespace Loupedeck.VoiceMeeterPlugin.Commands.Bases
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;

    using Extensions;

    using Helper;

    using Library.Voicemeeter;

    public class SingleBaseCommand : PluginDynamicCommand
    {
        private String ActionName { get; }
        private Action Action { get; }

        public SingleBaseCommand(String actionName, String description, String groupName, Action action) : base(
            actionName, description, groupName)
        {
            this.ActionName = actionName;
            this.Action = action;
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
            => DrawingHelper.DrawDefaultImage(this.ActionName, "", Color.DimGray);

        protected override void RunCommand(String actionParameter) => this.Action();
    }
}