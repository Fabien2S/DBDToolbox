using System;
using System.Runtime.InteropServices;

namespace DBDToolbox.Runtime.Platforms.Windows
{
    internal static class WinConsole
    {
        private const int StdOutputHandle = -11;
        private const uint EnableVirtualTerminalProcessing = 0x0004;

        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        public static bool EnableColors()
        {
            var handle = GetStdHandle(StdOutputHandle);
            return GetConsoleMode(handle, out var mode) &&
                   SetConsoleMode(handle, mode | EnableVirtualTerminalProcessing);
        }

        public static bool DisableColors()
        {
            var handle = GetStdHandle(StdOutputHandle);
            return GetConsoleMode(handle, out var mode) &&
                   SetConsoleMode(handle, mode & ~EnableVirtualTerminalProcessing);
        }
    }
}