using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Xml.Linq;
using UpTool2.DataStructures;

namespace UpTool2.Tool
{
    internal static class AppInstall
    {
        public static void Install(App appI)
        {
            string app = "";
            string tmp = "";
#if !DEBUG
            try
            {
#endif
                app = appI.appPath;
                tmp = PathTool.tempPath;
                if (Directory.Exists(tmp))
                    Directory.Delete(tmp, true);
                Directory.CreateDirectory(tmp);
                if (Directory.Exists(app))
                    Directory.Delete(app, true);
                Directory.CreateDirectory(app);
                using DownloadDialog dlg = new DownloadDialog(appI.File);
                if (dlg.ShowDialog() != DialogResult.OK)
                    throw new Exception("Download failed");
                using (SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider())
                {
                    string pkgHash = BitConverter.ToString(sha256.ComputeHash(dlg.Result)).Replace("-", string.Empty)
                        .ToUpper();
                    if (pkgHash != appI.Hash.ToUpper())
                        throw new Exception($@"The hash is not equal to the one stored in the repo:
Package: {pkgHash}
Online: {appI.Hash.ToUpper()}");
                }
                File.WriteAllBytes(Path.Combine(app, "package.zip"), dlg.Result);
                CompleteInstall(appI);
#if !DEBUG
            }
            catch
            {
                if (Directory.Exists(app))
                    Directory.Delete(app, true);
                throw;
            }
            finally
            {
#endif
                if (tmp != "" && Directory.Exists(tmp))
                    Directory.Delete(tmp, true);
#if !DEBUG
            }
#endif
        }

        public static void InstallZip(string zipPath, App meta)
        {
            string app = "";
            string tmp = "";
            try
            {
                app = meta.appPath;
                Directory.CreateDirectory(app);
                tmp = PathTool.tempPath;
                if (Directory.Exists(tmp))
                    Directory.Delete(tmp, true);
                Directory.CreateDirectory(tmp);
                File.Copy(zipPath, Path.Combine(app, "package.zip"));
                CompleteInstall(meta);
            }
            catch
            {
                if (Directory.Exists(app))
                    Directory.Delete(app, true);
                throw;
            }
            finally
            {
                if (tmp != "" && Directory.Exists(tmp))
                    Directory.Delete(tmp, true);
            }
        }

        //Use
        //PowerShell -Command "Add-Type -AssemblyName PresentationFramework;[System.Windows.MessageBox]::Show('Hello World')"
        //for message boxes
        private static void CompleteInstall(App app) => CompleteInstall(app.appPath, app.Name, app.Description, app.Version, app.MainFile);

        private static void CompleteInstall(string appPath, string name, string description, Version version, string mainFile)
        {
            string tmp = PathTool.tempPath;
            ZipFile.ExtractToDirectory(Path.Combine(appPath, "package.zip"), tmp);
            Directory.Move(Path.Combine(tmp, "Data"), Path.Combine(appPath, "app"));
            XElement el = new XElement("app", new XElement("Name", name), new XElement("Description", description),
                new XElement("Version", version));
            if (mainFile != null)
                el.Add(new XElement(new XElement("MainFile", mainFile)));
            el.Save(Path.Combine(appPath, "info.xml"));
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C \"{Path.Combine(tmp, "Install.bat")}\"",
                WorkingDirectory = Path.Combine(appPath, "app"),
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }).WaitForExit();
            if (GlobalVariables.RelE)
                GlobalVariables.ReloadElements.Invoke();
        }
    }
}