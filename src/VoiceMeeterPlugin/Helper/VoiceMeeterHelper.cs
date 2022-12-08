namespace Loupedeck.VoiceMeeterPlugin.Helper
{
    using System;

    using Library.Voicemeeter;

    public static class VoiceMeeterHelper
    {
        public static Int32 GetHardwareInputCount()
        {
            var version = Remote.Version;
            switch (version)
            {
                case RunVoicemeeterParam.VoicemeeterPotato:
                    return 5;
                default:
                    return 3;
            }
        }

        public static Int32 GetVirtualInputCount()
        {
            var version = Remote.Version;
            switch (version)
            {
                case RunVoicemeeterParam.VoicemeeterPotato:
                    return 3;
                default:
                    return 2;
            }
        }

        public static Int32 GetMasterSectionCount()
        {
            var version = Remote.Version;
            switch (version)
            {
                case RunVoicemeeterParam.VoicemeeterPotato:
                    return 8;
                default:
                    return 5;
            }
        }

        public static Int32 GetStripACount()
        {
            var version = Remote.Version;
            switch (version)
            {
                case RunVoicemeeterParam.VoicemeeterPotato:
                    return 5;
                default:
                    return 3;
            }
        }

        public static Int32 GetStripBCount()
        {
            var version = Remote.Version;
            switch (version)
            {
                case RunVoicemeeterParam.VoicemeeterPotato:
                    return 3;
                default:
                    return 2;
            }
        }
    }
}