namespace Loupedeck.VoiceMeeterPlugin.Actions.Bases
{
    using System;

    using Helper;

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
            => DrawingHelper.DrawDefaultImage(this.ActionName, "", ColorHelper.Inactive);

        protected override void RunCommand(String actionParameter) => this.Action();
    }
}