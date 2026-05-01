namespace Loupedeck.VoiceMeeterPlugin
{
    using Services;

    // This class can be used to connect the Loupedeck plugin to an application.

    public class VoiceMeeterApplication : ClientApplication
    {
        public VoiceMeeterApplication() => VoiceMeeterService.Instance.StartService(this).GetAwaiter().GetResult();

        private static readonly String[] SupportedProcessNames =
        [
            "voicemeeter",
            "voicemeeter_x64",
            "voicemeeterpro",
            "voicemeeterpro_x64",
            "voicemeeter8",
            "voicemeeter8x64",
        ];

        // This method can be used to link the plugin to a Windows application.
        protected override String GetProcessName() => "voicemeeter";

        // This method can be used to link the plugin to a macOS application.
        protected override String GetBundleName() => "";

        // This method can be used to check whether the application is installed or not.
        public override ClientApplicationStatus GetApplicationStatus() => ClientApplicationStatus.Unknown;

        protected override Boolean IsProcessNameSupported(String processName) =>
            SupportedProcessNames.Any(processName.ContainsNoCase);
    }
}
