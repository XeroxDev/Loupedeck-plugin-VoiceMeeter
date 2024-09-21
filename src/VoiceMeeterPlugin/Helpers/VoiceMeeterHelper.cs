namespace Loupedeck.VoiceMeeterPlugin.Helpers
{
    using Library.Voicemeeter;

    public static class VoiceMeeterHelper
    {
        public static Int32 GetHardwareInputCount()
        {
            var version = Remote.Version;
            return version switch
            {
                RunVoicemeeterParam.VoicemeeterPotato => 5,
                _ => 3
            };
        }

        public static Int32 GetVirtualInputCount()
        {
            var version = Remote.Version;
            return version switch
            {
                RunVoicemeeterParam.VoicemeeterPotato => 3,
                _ => 2
            };
        }

        public static Int32 GetMasterSectionCount()
        {
            var version = Remote.Version;
            return version switch
            {
                RunVoicemeeterParam.VoicemeeterPotato => 8,
                _ => 5
            };
        }

        public static Int32 GetStripACount()
        {
            var version = Remote.Version;
            return version switch
            {
                RunVoicemeeterParam.VoicemeeterPotato => 5,
                _ => 3
            };
        }

        public static Int32 GetStripBCount()
        {
            var version = Remote.Version;
            return version switch
            {
                RunVoicemeeterParam.VoicemeeterPotato => 3,
                _ => 2
            };
        }
    }
}