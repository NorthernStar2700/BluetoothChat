using System.Text.Json;
using System.Threading.Tasks;


namespace BluetoothChat.Models
{
    public static class ChatProtocol
    {
        public static string Serialize(ChatMessage message)
        {
            return JsonSerializer.Serialize(message);
        }

        public static ChatMessage Deserialize(string json)
        {
            return (ChatMessage) JsonSerializer.Deserialize(json, typeof(ChatMessage));
        }
    }
}
