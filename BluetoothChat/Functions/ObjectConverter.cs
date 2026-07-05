using BluetoothChat.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace BluetoothChat.Functions
{
    public static class ObjectConverter
    {
        public static string SerializeAccountMembers(List<AppAccount> accounts) => JsonConvert.SerializeObject(accounts);

        public static List<AppAccount> DeserializeAccountMembers(string json)
        {
            List<AppAccount> accounts = JsonConvert.DeserializeObject<List<AppAccount>>(json);
            return accounts ?? throw new IOException("Invalid or empty member list was passed in");
        }

        public static string SerializeMessage(ChatMessage message) => JsonConvert.SerializeObject(message);

        public static ChatMessage DeserializeMessage(string json)
        {
            ChatMessage message = JsonConvert.DeserializeObject<ChatMessage>(json);
            return message ?? throw new IOException("Invalid or empty chat message was passed in");
        }

        public static string SerializeSessionKeys(SessionKeys keys) => JsonConvert.SerializeObject(keys);

        public static SessionKeys DeserializeSessionKeys(string json)
        {
            SessionKeys keys = JsonConvert.DeserializeObject<SessionKeys>(json);
            return keys ?? throw new IOException("Invalid or empty session keys were passed in");
        }
    }
}
