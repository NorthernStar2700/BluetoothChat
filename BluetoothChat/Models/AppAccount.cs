using System;

namespace BluetoothChat.Models
{
    public class AppAccount
    {
        public string Name { get; set; }
        public string AccountId { get; set; }

        public void InitializeAccountId()
        {
            AccountId = Guid.NewGuid().ToString();
        }

        override public string ToString()
        {
            return Name;
        }
    }
}