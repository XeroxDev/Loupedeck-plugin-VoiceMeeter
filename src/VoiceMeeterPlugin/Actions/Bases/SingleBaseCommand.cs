namespace Loupedeck.VoiceMeeterPlugin.Actions.Bases
{
    using Helpers;

    public class SingleBaseCommand(String actionName, String description, String groupName, Action action)
        : PluginDynamicCommand(actionName, description, groupName)
    {
        private String ActionName { get; } = actionName;
        private Action Action { get; } = action;

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
            => DrawingHelper.DrawDefaultImage(this.ActionName, "", ColorHelper.Inactive);

        protected override void RunCommand(String actionParameter) => this.Action();
    }
}