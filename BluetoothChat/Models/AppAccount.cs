using System;

namespace BluetoothChat.Models
{
    public class AppAccount
    {
        public string Name { get; set; }
        public string AccountId { get; set; }

        public void InitializeAccountId()
        {
            if (string.IsNullOrWhiteSpace(AccountId))
            {
                AccountId = Guid.NewGuid().ToString();
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}