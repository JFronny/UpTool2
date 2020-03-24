using System;
using System.Collections.Generic;
using System.Text;
using UpToolLib.DataStructures;

namespace UpToolLib
{
    public static class ExternalFunctionalityManager
    {
        internal static IExternalFunctionality instance;

        public static void Init(IExternalFunctionality externalFunctionality) =>
            instance = externalFunctionality;
    }
}
