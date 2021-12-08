namespace Loupedeck.VoiceMeeterPlugin.Extensions
{
    using System;
    using System.Text.RegularExpressions;

    using Enums;

    public static class PluginExtensions
    {
        public static void SetStatus(this Plugin plugin, PluginStatus status, ErrorCode errorCode = ErrorCode.None)
        {
            if (plugin == null)
            {
                throw new ArgumentNullException(nameof(plugin));
            }

            if (status == PluginStatus.Error)
            {
                if (errorCode == 0)
                {
                    throw new ArgumentNullException(nameof(errorCode));
                }
            }

            var message = Regex.Replace(errorCode.ToString(), "[a-z][A-Z]",
                m => $"{m.Value[0]} {Char.ToLower(m.Value[1])}");

            if (plugin.PluginStatus.Status == status && plugin.PluginStatus.Message == message)
            {
                return;
            }

            plugin.OnPluginStatusChanged(
                status,
                message,
                $"https://help.xeroxdev.de/en/loupedeck/voicemeeter/error/{(UInt16)errorCode}"
            );
        }

        public static void ResetStatus(this Plugin plugin)
        {
            plugin.SetStatus(PluginStatus.Normal);
        }
    }
}