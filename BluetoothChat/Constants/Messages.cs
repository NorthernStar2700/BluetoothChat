using System;

namespace BluetoothChat.Constants
{
    public static class Messages
    {
        public static readonly Guid Guid = new Guid("3831e749-fe75-4c2e-a5d3-06bc74a68b50");
        public static readonly string BluetoothPrompt = "Enter Bluetooth address: ";
        public static readonly string ConsolePrompt = string.Format("Connect to or create a server using the \"Server\" tab. " +
            "{0}{0}Look up a devices address using the \"Address Lookup\" tab. {0}{0}Change your display name using the \"Username\" tab.", Environment.NewLine);
    }
}
