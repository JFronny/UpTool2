using System;
using System.Collections.Generic;
using UpToolLib.DataStructures;

namespace UpToolLib
{
    public static class GlobalVariables
    {
        public static readonly Dictionary<Guid, App> Apps = new Dictionary<Guid, App>();
        public static Version MinimumVer => Version.Parse("0.0.0.0");
    }
}