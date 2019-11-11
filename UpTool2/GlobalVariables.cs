using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace UpTool2
{
    static partial class GlobalVariables
    {
        public static Dictionary<Guid, App> apps = new Dictionary<Guid, App>();
        public static string dir => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\UpTool2";
        public static string appsPath => dir + @"\Apps";
        public static string getAppPath(App app) => getAppPath(app.ID);
        public static string getDataPath(App app) => getDataPath(app.ID);
        public static string getInfoPath(App app) => getInfoPath(app.ID);
        public static string getAppPath(Guid app) => appsPath + @"\" + app.ToString();
        public static string getDataPath(Guid app) => getAppPath(app) + @"\app";
        public static string getInfoPath(Guid app) => getAppPath(app) + "\\info.xml";
        public static bool relE = true;
    }
}
