using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BluetoothChat.Models
{
    public static class ChatProtocol
    {
        private const int MaxMessageLength = 64 * 1024; 

        public static async Task SendAsync(NetworkStream stream, ChatMessage message)
        {   
            
            try
            {
                string json = Serialize(message);
                byte[] messageData = Encoding.UTF8.GetBytes(json);

                // HostToNetworkOrder helps with different platforms and message lengths
                byte[] lengthData = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(messageData.Length));

                // Tell the server the length of the message as well as the message itself
                await WriteInternalAsync(stream, lengthData, 0, lengthData.Length);
                await WriteInternalAsync(stream, messageData, 0, messageData.Length);
            }
            catch (Exception e)
            {
                throw new IOException($"Write message error: {e.Message}", e);
            }
        }

        public static async Task<ChatMessage> ReadAsync(NetworkStream stream)
        {
            try
            {
                // 4 is used as a length buffer
                byte[] lengthBuffer = await ReadBytesAsync(stream, 4);
                int messageLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lengthBuffer, 0));

                // Check message length to make sure it isn't empty or exceeds a certain amount
                if (messageLength <= 0 || messageLength > MaxMessageLength)
                {
                    throw new IOException($"Invalid message length: {messageLength}");
                }

                byte[] messageBuffer = await ReadBytesAsync(stream, messageLength);
                string json = Encoding.UTF8.GetString(messageBuffer);

                return Deserialize(json);
            }
            catch (Exception e)
            {
                throw new IOException($"Read message error: {e.Message}", e);
            }
        }

        public static string SerializeAccountMembers(List<AppAccount> accounts) => JsonConvert.SerializeObject(accounts);

        public static List<AppAccount> DeserializeAccountMembers(string json) => (List<AppAccount>) JsonConvert.DeserializeObject(json, typeof(List<AppAccount>));

        private static async Task<byte[]> ReadBytesAsync(NetworkStream stream, int length)
        {
            byte[] buffer = new byte[length];
            int totalRead = 0;

            while (totalRead < length)
            {
                // This ReadInternalAsync is used for Mono compatability. We are on another thread
                int bytesRead = await ReadInternalAsync(stream, buffer, totalRead, length - totalRead);

                if (bytesRead == 0)
                {
                    throw new IOException("Connection was closed");
                }

                totalRead += bytesRead;
            }

            return buffer;
        }

        private static Task<int> ReadInternalAsync(NetworkStream stream, byte[] buffer, int offset, int count) => Task.Run(() => stream.Read(buffer, offset, count));

        private static Task WriteInternalAsync(NetworkStream stream, byte[] buffer, int offset, int count) => Task.Run(() => stream.Write(buffer, offset, count));

        private static string Serialize(ChatMessage message) => JsonConvert.SerializeObject(message);

        private static ChatMessage Deserialize(string json)
        {
            ChatMessage message = (ChatMessage) JsonConvert.DeserializeObject(json, typeof(ChatMessage));
            return message ?? throw new IOException("Invalid or empty chat message was passed in");
        }
    }
}
