using System;

namespace BluetoothChat.Constants
{
    public static class UIMessages
    {
        public static readonly string ConsolePrompt = string.Format("Connect to or create a server using the \"Server\" tab. " +
    "{0}{0}Look up a devices address using the \"Address Lookup\" tab. {0}{0}Change your display name using the \"Username\" tab.", Environment.NewLine);
        public static readonly string UsernameMessage = "Current Username: ";
        public const string BluetoothPrompt = "Enter Bluetooth address: ";
    }
}
