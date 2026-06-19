using InTheHand.Net.Sockets;
using System.Net.Sockets;
using System.Threading;

namespace BluetoothChat.Models
{
    // A class for managing a user that connects within the application
    public class ClientSession
    {
        public AppAccount Account { get; set; }
        public BluetoothClient Client { get; set; }
        public NetworkStream Stream { get; set; }
        public SemaphoreSlim Lock { get; } = new SemaphoreSlim(1, 1);
    }
}
