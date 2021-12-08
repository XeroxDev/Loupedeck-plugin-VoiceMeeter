namespace Loupedeck.VoiceMeeterPlugin.Library.Voicemeeter
{
    using System;
    using System.Runtime.InteropServices;

    internal static class Wrapper
    {
        // Load the DLL to pull it into the running process
        [DllImport("kernel32.dll")]
        internal static extern IntPtr LoadLibrary(String dllToLoad);
    }
}