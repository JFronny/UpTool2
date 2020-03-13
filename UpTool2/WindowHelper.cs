using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace UpTool2
{
    //TODO: This class is Windows-only and can't be used cross-platform.
    public static class WindowHelper
    {
        public static void BringProcessToFront(Process process)
        {
            IntPtr handle = process.MainWindowHandle;
            if (IsIconic(handle))
                ShowWindow(handle, 9);
            SetForegroundWindow(handle);
        }

        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr handle);

        [DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr handle, int nCmdShow);

        [DllImport("User32.dll")]
        private static extern bool IsIconic(IntPtr handle);
    }
}