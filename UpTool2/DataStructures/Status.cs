using System;

namespace UpTool2.DataStructures
{
    [Flags]
    public enum Status
    {
        Not_Installed = 1,
        Updatable = 2,
        Installed = 4,
        Local = 8,
        All = 15
    }
}