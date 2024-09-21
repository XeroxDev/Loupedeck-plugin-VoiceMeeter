namespace Loupedeck.VoiceMeeterPlugin
{
    using Helpers;

    using Library.Voicemeeter;

    // This class contains the plugin-level logic of the Loupedeck plugin.

    public class VoiceMeeterPlugin : Plugin
    {
        // Gets a value indicating whether this is an API-only plugin.
        public override Boolean UsesApplicationApiOnly => true;

        // Gets a value indicating whether this is a Universal plugin or an Application plugin.
        public override Boolean HasNoApplication => true;

        // Initializes a new instance of the plugin class.
        public VoiceMeeterPlugin()
        {
            // Initialize the plugin log.
            PluginLog.Init(this.Log);

            // Initialize the plugin resources.
            PluginResources.Init(this.Assembly);
        }

        // This method is called when the plugin is loaded.
        public override void Load()
        {
        }

        // This method is called when the plugin is unloaded.
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
    }
}