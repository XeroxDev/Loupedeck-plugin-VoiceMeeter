namespace Loupedeck.VoiceMeeterPlugin.Library.Voicemeeter
{
    using System;

    public enum RunVoicemeeterParam
    {
        None = 0,
        Voicemeeter = 1,
        VoicemeeterBanana = 2,
        VoicemeeterPotato = 3,
    };

    public enum LevelType
    {
        PreFaderInput = 0,
        PostFaderInput = 1,
        PostMuteInput = 2,
        Output = 3
    };

    public enum LoginResponse
    {
        AlreadyLoggedIn = -2,
        NoClient = -1,
        Ok = 0,
        VoicemeeterNotRunning = 1,
    }

    public static class InputChannel
    {
        public const Int32 Strip1Left = 0;
        public const Int32 Strip1Right = 1;
        public const Int32 Strip2Left = 2;
        public const Int32 Strip2Right = 3;
        public const Int32 Strip3Left = 4;
        public const Int32 Strip3Right = 5;
        public const Int32 VaioLeft = 6;
        public const Int32 VaioRight = 7;
        public const Int32 AuxLeft = 8;
        public const Int32 AuxRight = 9;
    }

    public static class OutputChannel
    {
        public const Int32 A1Left = 0;
        public const Int32 A1Right = 1;
        public const Int32 A2Left = 2;
        public const Int32 A2Right = 3;
        public const Int32 A3Left = 4;
        public const Int32 A3Right = 5;
        public const Int32 Bus1Left = 6;
        public const Int32 Bus1Right = 7;
        public const Int32 Bus2Left = 8;
        public const Int32 Bus2Right = 9;
    }

    public static class VoicemeeterCommand
    {
        public static String Shutdown = "Command.Shutdown";
        public static String Show = "Command.Show";
        public static String Restart = "Command.Restart";
        public static String Eject = "Command.Eject";
        public static String Reset = "Command.Reset";
        public static String Save = "Command.Save";
        public static String Load = "Command.Load";
    }
}