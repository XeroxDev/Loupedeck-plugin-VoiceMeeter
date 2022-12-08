namespace Loupedeck.VoiceMeeterPlugin.Enums
{
    using System;

    public enum ErrorCode : UInt16
    {
        None,
        NotConnected = 100,
        NotInstalled,
        ChannelOutOfRange,
        ParameterError,
        ParameterNotFound,
        StructureMismatch,
    }
}