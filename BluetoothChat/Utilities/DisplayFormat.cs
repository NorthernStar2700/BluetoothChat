using System;

namespace BluetoothChat.Utilities
{
    public static class DisplayFormat
    {
        public static string FormatConsoleMessage(string message) => $"{message}{Environment.NewLine}{Environment.NewLine}";
    }
}
