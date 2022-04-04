namespace Loupedeck.VoiceMeeterPlugin.Library.Voicemeeter
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Enums;

    using Extensions;

    using Microsoft.Win32;

    public static class Remote
    {
        // Don't keep loading the DLL
        private static IntPtr? _handle;
        private static ClientApplication ClientApplication { get; set; }

        public static RunVoicemeeterParam Version { get; set; }

        #region Parameters

        /// <summary>
        /// Gets a text value
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static String GetTextParameter(String parameter)
        {
            var buffer = new StringBuilder(255);
            var code = RemoteWrapper.InternalGetParameterW(parameter, buffer);
            if (code == 0)
            {
                return buffer.ToString();
            }

            return null;
        }

        /// <summary>
        /// Set a text value
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static void SetTextParameter(String parameter, String value) =>
            TestResult(RemoteWrapper.InternalSetParameterW(parameter, value));

        /// <summary>
        /// Get a named parameter
        /// </summary>
        /// <param name="parameter">Parameter name</param>
        /// <returns>float value</returns>
        public static Single GetParameter(String parameter)
        {
            Single value = 0;
            TestResult(RemoteWrapper.GetParameter(parameter, ref value));
            return value;
        }

        /// <summary>
        /// Set a named parameter
        /// </summary>
        /// <param name="parameter">Parameter name</param>
        /// <param name="value">float value</param>
        public static void SetParameter(String parameter, Single value) =>
            TestResult(RemoteWrapper.SetParameter(parameter, value));


        /// <summary>
        /// Set one or several parameters by a script
        /// </summary>
        /// <param name="parameters">One or more instructions separated by comma, semicolon or newline</param>
        public static void SetParameters(String parameters) => TestResult(RemoteWrapper.SetParameters(parameters));

        #endregion

        #region Commands

        /// <summary>
        /// Start the VoiceMeeter program
        /// </summary>
        /// <param name="voicemeterType"></param>
        public static void Start(RunVoicemeeterParam voicemeterType)
        {
            switch (RemoteWrapper.InternalRunVoicemeeter((Int32)voicemeterType))
            {
                case 0: return;
                case -1:
                    SendError(ErrorCode.NotInstalled);
                    return;
                default:
                    SendError();
                    return;
            }
        }

        /// <summary>
        /// Shutdown the VoiceMeeter program
        /// </summary>
        public static void Shutdown() => TestResult(RemoteWrapper.SetParameter(VoicemeeterCommand.Shutdown, 1));

        /// <summary>
        /// Restart the audio engine
        /// </summary>
        public static void Restart() => TestResult(RemoteWrapper.SetParameter(VoicemeeterCommand.Restart, 1));

        /// <summary>
        /// Shows the running Voicemeeter application if minimized.
        /// </summary>
        public static void Show() => TestResult(RemoteWrapper.SetParameter(VoicemeeterCommand.Show, 1));

        /// <summary>
        /// Return if the parameters have changed since the last time this method was called.
        /// </summary>
        public static Int32 IsParametersDirty()
        {
            try
            {
                return RemoteWrapper.IsParametersDirty();
            }
            catch (Exception)
            {
                // TODO: Figure out the Memory Exception when calling the API
            }

            return 0;
        }

        /// <summary>
        /// Eject Cassette
        /// </summary>
        public static void Eject() => TestResult(RemoteWrapper.SetParameter(VoicemeeterCommand.Eject, 1));
        
        /// <summary>
        /// Reset all configuration
        /// </summary>
        public static void Reset() => TestResult(RemoteWrapper.SetParameter(VoicemeeterCommand.Reset, 1));

        /// <summary>
        /// Load a configuation file name
        /// </summary>
        /// <param name="configurationFileName">Full path to file</param>
        public static void Load(String configurationFileName) =>
            SetTextParameter(VoicemeeterCommand.Load, configurationFileName);

        /// <summary>
        /// Save a configuration to the given file name
        /// </summary>
        /// <param name="configurationFileName">Full path to file</param>
        public static void Save(String configurationFileName) =>
            SetTextParameter(VoicemeeterCommand.Load, configurationFileName);

        #endregion

        #region Rx

        public static Single GetLevel(LevelType type, Int32 channel)
        {
            Single value = 0;
            TestLevelResult(RemoteWrapper.GetLevel((Int32)type, channel, ref value));
            return value;
        }

        private static void TestLevelResult(Int32 result)
        {
            // 0: OK(no error).
            // -1: error
            // -2: no server.
            // -3: no level available
            // -4: out of range
            switch (result)
            {
                case 0: return;
                case -1:
                    SendError();
                    return;
                case -2:
                    SendError(ErrorCode.NotConnected);
                    return;
                case -3:
                    return;
                case -4:
                    SendError(ErrorCode.ChannelOutOfRange);
                    return;
                default:
                    SendError();
                    return;
            }
        }

        #endregion

        /// <summary>
        /// Logs into the Voicemeeter application.  Starts the given application (Voicemeeter, Bananna, Potato) if it is not already runnning.
        /// </summary>
        /// <param name="voicemeeterType">The Voicemeeter program to run</param>
        /// <param name="application"></param>
        /// <returns>IDisposable class to dispose when finished with the remote.</returns>
        public static async Task<IDisposable> Initialize(RunVoicemeeterParam voicemeeterType,
            ClientApplication application)
        {
            ClientApplication = application;
            if (_handle.HasValue == false)
            {
                // Find current version from the registry
                const String key = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
                const String key32 =
                    @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
                const String uninstKey = "VB:Voicemeeter {17359A74-1236-5467}";
                var voicemeeter = Registry.GetValue($"{key}\\{uninstKey}", "UninstallString", null);

                if (voicemeeter == null && Environment.Is64BitProcess)
                {
                    // Fall back to 32-bits registry
                    voicemeeter = Registry.GetValue($"{key32}\\{uninstKey}", "UninstallString", null);
                }

                if (voicemeeter == null)
                {
                    SendError(ErrorCode.NotInstalled);
                    return null;
                }

                var voicemeeterString = voicemeeter.ToString().Split('\\').Last();
                var directoryName = Path.GetDirectoryName(voicemeeter.ToString());
                if (directoryName == null)
                {
                    SendError(ErrorCode.NotInstalled);
                    return null;
                }

                if (voicemeeterString.ContainsNoCase('8'))
                {
                    Version = RunVoicemeeterParam.VoicemeeterPotato;
                }
                else if (voicemeeterString.ContainsNoCase("pro"))
                {
                    Version = RunVoicemeeterParam.VoicemeeterBanana;
                }
                else
                {
                    //Maybe their install EXE was named something different:)
                    var potatoPanelFile = Path.Combine(directoryName, "VBVMVAIO3_ControlPanel.exe");
                    var bananaPanelFile = Path.Combine(directoryName, "VBVMAUX_ControlPanel.exe");
                    if (File.Exists(potatoPanelFile))
                    {
                        Version = RunVoicemeeterParam.VoicemeeterPotato;
                    }
                    else if (File.Exists(bananaPanelFile))
                    {
                        Version = RunVoicemeeterParam.VoicemeeterBanana;
                    }
                    else
                    {
                        Version = RunVoicemeeterParam.Voicemeeter;
                    }
                }


                _handle = Wrapper.LoadLibrary(
                    Path.Combine(directoryName,
                        Environment.Is64BitProcess ? "VoicemeeterRemote64.dll" : "VoicemeeterRemote.dll"));
            }

            var startVoiceMeeter = voicemeeterType != RunVoicemeeterParam.None;

            if (await Login(voicemeeterType, startVoiceMeeter).ConfigureAwait(false))
            {
                return new VoicemeeterClient();
            }

            return null;
        }

        public static async Task<Boolean> Login(RunVoicemeeterParam voicemeeterType, Boolean retry = true)
        {
            while (true)
            {
                switch ((LoginResponse)RemoteWrapper.LoginVoicemeeter())
                {
                    case LoginResponse.Ok:
                    case LoginResponse.AlreadyLoggedIn:
                        return true;

                    case LoginResponse.VoicemeeterNotRunning:
                        if (retry)
                        {
                            // Run voicemeeter program
                            Start(voicemeeterType);

                            await Task.Delay(2000).ConfigureAwait(false);
                            retry = false;
                            continue;
                        }

                        break;
                }

                return false;
            }
        }

        private static void TestResult(Int32 result)
        {
            //0: OK(no error).
            //-1: error
            //-2: no server.
            //-3: unknown parameter
            //-5: structure mismatch
            switch (result)
            {
                case 0: return;
                case -1:
                    SendError(ErrorCode.ParameterError);
                    return;
                case -2:
                    SendError(ErrorCode.NotConnected);
                    return;
                case -3:
                    SendError(ErrorCode.ParameterNotFound);
                    return;
                case -5:
                    SendError(ErrorCode.StructureMismatch);
                    return;
                default:
                    SendError();
                    return;
            }
        }

        private static void SendError(ErrorCode errorCode = ErrorCode.None)
            => ClientApplication?.Plugin.SetStatus(PluginStatus.Error, errorCode);
    }
}