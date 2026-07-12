using BluetoothChat.Models;
using BluetoothChat.Utilities;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothChat.Functions
{
    public static class ChatProtocol
    {
        // A preset length for messages ensuring nothing goes beyond a certain point
        private const int MaxMessageLength = 64 * 1024; 

        public static async Task SendUnencryptedAsync(NetworkStream stream, ChatMessage message)
        {
            if (stream == null || message == null || !stream.CanWrite)
            {
                throw new IOException("Cannot write to client");
            }

            try
            {
                // Try to convert the message into JSON format
                string json = ObjectConverter.SerializeMessage(message);
                byte[] messageData = Encoding.UTF8.GetBytes(json);

                // HostToNetworkOrder is used as the clients will be sending data to the network (server)
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

        public static async Task SendAsync(NetworkStream stream, ChatMessage message, byte[] aesKey)
        {   
            if (stream == null || message == null || !stream.CanWrite)
            {
                throw new IOException("Cannot write to client");
            }

            try
            {
                // Try to convert the message into JSON format
                string json = ObjectConverter.SerializeMessage(message);
                byte[] messageData = await SecureMessageProtocol.EncryptAsync(json, aesKey);

                // HostToNetworkOrder is used as the clients will be sending data to the network (server)
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

        public static async Task<ChatMessage> ReadUnencryptedAsync(NetworkStream stream)
        {
            if (stream == null || !stream.CanRead)
            {
                throw new IOException("Cannot read from client");
            }

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

                return ObjectConverter.DeserializeMessage(json);
            }
            catch (Exception e)
            {
                throw new IOException($"Read message error: {e.Message}", e);
            }
        }

        public static async Task<ChatMessage> ReadAsync(NetworkStream stream, byte[] aesKey)
        {
            if (stream == null || !stream.CanRead)
            {
                throw new IOException("Cannot read from client");
            }

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
                string json = await SecureMessageProtocol.DecryptAsync(messageBuffer, aesKey);

                return ObjectConverter.DeserializeMessage(json);
            }
            catch (Exception e)
            {
                throw new IOException($"Read message error: {e.Message}", e);
            }
        }

        private static async Task<byte[]> ReadBytesAsync(NetworkStream stream, int length)
        {
            byte[] buffer = new byte[length];
            int totalRead = 0;

            while (totalRead < length)
            {
                // This ReadInternalAsync is used for Mono compatability. We are on another thread
                // Read bytes as they come in, return the total after
                int bytesRead = await ReadInternalAsync(stream, buffer, totalRead, length - totalRead);

                if (bytesRead == 0)
                {
                    throw new IOException("Connection was closed");
                }

                totalRead += bytesRead;
            }

            return buffer;
        }

        private static Task<int> ReadInternalAsync(NetworkStream stream, byte[] buffer, int offset, int count) 
            => Task.Run(() => stream.Read(buffer, offset, count));

        private static Task WriteInternalAsync(NetworkStream stream, byte[] buffer, int offset, int count) 
            => Task.Run(() => stream.Write(buffer, offset, count));
    }
}
