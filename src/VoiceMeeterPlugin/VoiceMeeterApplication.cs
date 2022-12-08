namespace Loupedeck.VoiceMeeterPlugin
{
    using System;

    using Services;

    // This class can be used to connect the Loupedeck plugin to an application.

    public class VoiceMeeterApplication : ClientApplication
    {
        public VoiceMeeterApplication() => VoiceMeeterService.Instance.StartService(this).ConfigureAwait(true);

        // This method can be used to link the plugin to a Windows application.
        protected override String GetProcessName() => "";

        // This method can be used to link the plugin to a macOS application.
        protected override String GetBundleName() => "";

        // This method can be used to check whether the application is installed or not.
        public override ClientApplicationStatus GetApplicationStatus() => ClientApplicationStatus.Unknown;

        protected override Boolean IsProcessNameSupported(String processName) =>
            processName.ContainsNoCase("VB-AUDIO Virtual Audi Device") || processName.ContainsNoCase("VoiceMeeter");
    }
}
