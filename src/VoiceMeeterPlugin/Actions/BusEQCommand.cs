﻿namespace Loupedeck.VoiceMeeterPlugin.Actions
{
    using Bases;

    using Helpers;

    public class BusEqCommand : BooleanBaseCommand
    {
        public BusEqCommand() : base(true, false) =>
            this.CreateCommands(
                VoiceMeeterHelper.GetHardwareInputCount() + VoiceMeeterHelper.GetVirtualInputCount(),
                "EQ.on",
                0,
                "EQ"
            ).ConfigureAwait(false);
    }
}