using System;

namespace BluetoothChat.Utilities
{
    public static class DisplayFormat
    {
        public static string FormatMessage(string message)
        {
            return $"{message}{Environment.NewLine}{Environment.NewLine}";
        }
    }
}
