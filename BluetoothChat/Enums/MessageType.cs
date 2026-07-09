namespace BluetoothChat.Enums
{
    public enum MessageType
    {
        // Client
        Chat,
        Join,
        Leave,
        ClientAesKey,
        HandshakeRequested,

        // Server
        ServerMessage,
        ServerPublicKey,
        MemberList,
        HandshakeComplete,

        // Shared
        UsernameChange
    }
}
