using System;
using System.Collections.Generic;
using UpTool2.DataStructures;

namespace UpTool2
{
    internal static class GlobalVariables
    {
        public static readonly Dictionary<Guid, App> Apps = new Dictionary<Guid, App>();
        public static bool RelE = true;
        public static Action ReloadElements;
        public static Version minimumVer => Version.Parse("0.0.0.0");
    }
}