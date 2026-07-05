using InTheHand.Net.Sockets;
using System;
using System.Net.Sockets;
using System.Threading;

namespace BluetoothChat.Models
{
    // A class for managing a user that connects within the application
    public class ClientSession : IDisposable
    {
        public AppAccount Account { get; set; }
        public BluetoothClient Client { get; set; }
        public NetworkStream Stream { get; set; }
        public SemaphoreSlim SendLock { get; } = new SemaphoreSlim(1, 1);
        public bool IsSecure { get; set; }

        public void Dispose()
        {
            Stream?.Dispose();
            Client?.Close();
            Client?.Dispose();
            SendLock?.Dispose();
        }
    }
}
