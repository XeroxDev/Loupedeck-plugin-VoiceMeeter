namespace Loupedeck.VoiceMeeterPlugin.Extensions
{
    using System.Text.RegularExpressions;

    using Enums;

    public static partial class PluginExtensions
    {
        public static void SetStatus(this Plugin plugin, PluginStatus status, ErrorCode errorCode = ErrorCode.None)
        {
            ArgumentNullException.ThrowIfNull(plugin);

            if (status == PluginStatus.Error)
            {
                if (errorCode == 0)
                {
                    throw new ArgumentNullException(nameof(errorCode));
                }
            }

            var message = LettersRegex().Replace(errorCode.ToString(), m => $"{m.Value[0]} {Char.ToLower(m.Value[1])}");

            if (plugin.PluginStatus.Status == status && plugin.PluginStatus.Message == message)
            {
                return;
            }

            plugin.OnPluginStatusChanged(
                status,
                message,
                $"https://help.xeroxdev.de/en/loupedeck/voicemeeter/error/{(UInt16)errorCode}",
                $"Error {(UInt16)errorCode}"
            );
            
            // reset the status after 5 seconds
            if (status == PluginStatus.Error)
            {
                Task.Delay(5000).ContinueWith(_ => plugin.ResetStatus());
            }
        }

        public static void ResetStatus(this Plugin plugin) => plugin.SetStatus(PluginStatus.Normal);

        [GeneratedRegex("[a-z][A-Z]")]
        private static partial Regex LettersRegex();
    }
}