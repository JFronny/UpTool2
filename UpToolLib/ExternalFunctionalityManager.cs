using UpToolLib.DataStructures;

namespace UpToolLib
{
    public static class ExternalFunctionalityManager
    {
        internal static IExternalFunctionality Instance;

        public static void Init(IExternalFunctionality externalFunctionality)
        {
            Instance = externalFunctionality;
        }
    }
}