using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using UpTool2.Properties;
using UpToolLib;
using UpToolLib.DataStructures;
using UpToolLib.Tool;

namespace UpTool2.Task
{
    internal class UploadTask : IAppTask
    {
        private readonly string _name;
        private readonly Action? _postInstall;
        public readonly string ZipFile;

        public UploadTask(string zipFile, string name, Action? postInstall = null)
        {
            ZipFile = zipFile;
            _name = name;
            _postInstall = postInstall;
        }

        public void Run()
        {
#if !DEBUG
            try
            {
#endif
                Guid id = Guid.NewGuid();
                while (GlobalVariables.Apps.ContainsKey(id) || Directory.Exists(PathTool.GetAppPath(id)))
                    id = Guid.NewGuid();
                App appI = new App(_name, "Locally installed package, removal only",
                    GlobalVariables.MinimumVer, "", true, "", id, Color.Red, Resources.C_64.ToBitmap(), false, "");
                AppInstall.InstallZip(ZipFile, appI, true);
                _postInstall?.Invoke();
#if !DEBUG
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.ToString(), "Install failed");
            }
#endif
        }

        public override string ToString() => $"Install local {Path.GetFileName(ZipFile)}";
    }
}