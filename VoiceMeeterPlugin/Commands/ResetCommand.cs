namespace Loupedeck.VoiceMeeterPlugin.Commands
{
    using System;

    using Bases;

    using Library.Voicemeeter;

    public class ResetCommand : SingleBaseCommand
    {
        public ResetCommand() : base("Reset", "Reset ALL configuration of VoiceMeeter", "Special", Remote.Reset)
        {
        }
    }
}