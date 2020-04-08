using System;
using System.Linq;

namespace UpToolLib
{
    public static class PlatformCheck
    {
        public static bool IsWindows => new[]
            {
                PlatformID.Xbox, PlatformID.Win32S, PlatformID.Win32Windows, PlatformID.Win32NT,
                PlatformID.WinCE
            }
            .Contains(Environment.OSVersion.Platform);

        public static bool IsPosix => !IsWindows;
    }
}