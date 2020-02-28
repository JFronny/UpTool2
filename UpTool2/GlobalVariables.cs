using System;
using System.Collections.Generic;
using UpTool2.Data;

namespace UpTool2
{
    static partial class GlobalVariables
    {
        public static Dictionary<Guid, App> apps = new Dictionary<Guid, App>();
        public static bool relE = true;
        public static Action reloadElements;
        public static Version minimumVer => Version.Parse("0.0.0.0");
    }
}