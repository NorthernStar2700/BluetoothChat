using System;
using System.Net.Sockets;
using System.Text;
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

        public static async Task SendAsync(NetworkStream stream, ChatMessage message)
        {
            string json = Serialize(message);
            byte[] messageData = Encoding.UTF8.GetBytes(json);
            byte[] lengthData = BitConverter.GetBytes(messageData.Length);

            // Tell the server the length of the message as well as the message itself
            await stream.WriteAsync(lengthData, 0, lengthData.Length);
            await stream.WriteAsync(messageData, 0, messageData.Length);
        }

        public static async Task<ChatMessage> ReadAsync(NetworkStream stream)
        {
            // 4 is used as a length buffer
            byte[] lengthBuffer = await ReadBytesAsync(stream, 4);
            int messageLength = BitConverter.ToInt32(lengthBuffer, 0);

            byte[] messageBuffer = await ReadBytesAsync(stream, messageLength);
            string json = Encoding.UTF8.GetString(messageBuffer);

            return Deserialize(json);
        }

        private static async Task<byte[]> ReadBytesAsync(NetworkStream stream, int length)
        {
            byte[] buffer = new byte[length];
            int totalRead = 0;

            while (totalRead < length)
            {
                int bytesRead = await stream.ReadAsync(buffer, totalRead, length - totalRead);

                if (bytesRead == 0)
                {
                    throw new InvalidOperationException("Connection was closed");
                }

                totalRead += bytesRead;
            }

            return buffer;
        }
    }
}
