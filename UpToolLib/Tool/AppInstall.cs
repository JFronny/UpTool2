﻿using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Xml.Linq;
using UpToolLib.DataStructures;

namespace UpToolLib.Tool
{
    public static class AppInstall
    {
        /// <summary>
        /// Install an application
        /// </summary>
        /// <param name="appI">The app to install</param>
        /// <param name="force">Set to true to overwrite all old data</param>
        public static void Install(App appI, bool force)
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
                if (force)
                {
                    if (Directory.Exists(app))
                        Directory.Delete(app, true);
                    Directory.CreateDirectory(app);
                }
                else
                {
                    if (!Directory.Exists(app))
                        Directory.CreateDirectory(app);
                }
                (bool dlSuccess, byte[] dlData) = ExternalFunctionalityManager.instance.Download(new Uri(appI.File));
                if (!dlSuccess)
                    throw new Exception("Download failed");
                using (SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider())
                {
                    string pkgHash = BitConverter.ToString(sha256.ComputeHash(dlData)).Replace("-", string.Empty)
                        .ToUpper();
                    if (pkgHash != appI.Hash.ToUpper())
                        throw new Exception($@"The hash is not equal to the one stored in the repo:
Package: {pkgHash}
Online: {appI.Hash.ToUpper()}");
                }
                File.WriteAllBytes(Path.Combine(app, "package.zip"), dlData);
                CompleteInstall(appI, force);
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

        public static void InstallZip(string zipPath, App meta, bool force)
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
                CompleteInstall(meta, force);
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
        private static void CompleteInstall(App app, bool force) => CompleteInstall(app.appPath, app.Name, app.Description, app.Version, app.MainFile, force);

        private static void CompleteInstall(string appPath, string name, string description, Version version, string mainFile, bool force)
        {
            string tmp = PathTool.tempPath;
            ZipFile.ExtractToDirectory(Path.Combine(appPath, "package.zip"), tmp);
            if (force)
                Directory.Move(Path.Combine(tmp, "Data"), Path.Combine(appPath, "app"));
            else
            {
                CopyAll(Path.Combine(tmp, "Data"), Path.Combine(appPath, "app", "Data"));
                Directory.Delete(Path.Combine(tmp, "Data"), true);
            }
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
        }
        
        private static void CopyAll(string source, string target)
        {
            if (string.Equals(Path.GetFullPath(source), Path.GetFullPath(target), StringComparison.CurrentCultureIgnoreCase))
                return;
            if (!Directory.Exists(target))
                Directory.CreateDirectory(target);
            foreach (string file in Directory.GetFiles(source)) File.Copy(file, Path.Combine(target, Path.GetFileName(file)), true);
            foreach (string dir in Directory.GetDirectories(source))
                CopyAll(dir, Path.Combine(target, Path.GetFileName(dir)));
        }
    }
}