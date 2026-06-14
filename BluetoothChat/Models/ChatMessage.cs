using BluetoothChat.Enums;

namespace BluetoothChat.Models
{
    public class ChatMessage
    {
        public MessageType MessageType { get; set; }
        public string SenderName { get; set; }
        public string SenderId { get; set; }
        public string Message { get; set; }
        public bool IsHost { get; set; }
    }
}
