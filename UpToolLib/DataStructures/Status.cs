using System;

namespace UpToolLib.DataStructures
{
    [Flags]
    public enum Status
    {
        NotInstalled = 1,
        Updatable = 2,
        Installed = 4,
        Local = 8,
        All = 15
    }
}