namespace Loupedeck.VoiceMeeterPlugin.Enums
{
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