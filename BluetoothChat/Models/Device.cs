namespace BluetoothChat.Models
{
    public class Device
    {
        public string Name { get; set; }
        public string Address { get; set; }

        public override string ToString()
        {
            return $"{Name} | {Address}";
        }
    }
}
