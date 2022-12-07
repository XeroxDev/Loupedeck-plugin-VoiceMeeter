namespace Loupedeck.VoiceMeeterPlugin
{
    using System;

    using Services;

    public class VoiceMeeterApplication : ClientApplication
    {
        public VoiceMeeterApplication() => VoiceMeeterService.Instance.StartService(this).ConfigureAwait(true);


        protected override Boolean IsProcessNameSupported(String processName) =>
            processName.ContainsNoCase("VB-AUDIO Virtual Audi Device") || processName.ContainsNoCase("VoiceMeeter");

        protected override String GetBundleName() => "";
    }
}