﻿using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace UpTool2
{
    static class AppInstall
    {
        public static void Install(App appI)
        {
            string app = "";
            string tmp = "";
            try
            {
                app = GlobalVariables.dir + @"\Apps\" + appI.ID.ToString();
                tmp = GlobalVariables.dir + @"\tmp";
                if (Directory.Exists(""))
                    Directory.Delete("", true);
                Directory.CreateDirectory("");
                if (Directory.Exists(app))
                    Directory.Delete(app, true);
                Directory.CreateDirectory(app);
                if (new DownloadDialog(appI.file, app + @"\package.zip").ShowDialog() != DialogResult.OK)
                    throw new Exception("Download failed");
                using (SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider())
                {
                    string pkghash = BitConverter.ToString(sha256.ComputeHash(File.ReadAllBytes(app + @"\package.zip"))).Replace("-", string.Empty).ToUpper();
                    if (pkghash != appI.hash.ToUpper())
                        throw new Exception("The hash is not equal to the one stored in the repo:\r\nPackage: " + pkghash + "\r\nOnline: " + appI.hash.ToUpper());
                }
                completeInstall(app, appI);
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

        public static void installZip(string zipPath, App meta)
        {
            string app = "";
            string tmp = "";
            try
            {
                app = meta.appPath;
                Directory.CreateDirectory(app);
                tmp = GlobalVariables.dir + @"\tmp";
                if (Directory.Exists(tmp))
                    Directory.Delete(tmp, true);
                Directory.CreateDirectory(tmp);
                File.Copy(zipPath, app + @"\package.zip");
                completeInstall(app, meta);
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

        static void completeInstall(string app, App appI)
        {
#if !DEBUG
            try
            {
#endif
                string tmp = GlobalVariables.dir + @"\tmp";
                ZipFile.ExtractToDirectory(app + @"\package.zip", tmp);
                Directory.Move(tmp + @"\Data", app + @"\app");
                if (appI.runnable)
                    new XElement("app", new XElement("Name", appI.name), new XElement("Description", appI.description), new XElement("Version", appI.version), new XElement("MainFile", appI.mainFile)).Save(app + @"\info.xml");
                else
                    new XElement("app", new XElement("Name", appI.name), new XElement("Description", appI.description), new XElement("Version", appI.version)).Save(app + @"\info.xml");
                Process.Start(new ProcessStartInfo { FileName = "cmd.exe", Arguments = "/C \"" + tmp + "\\Install.bat\"", WorkingDirectory = app + @"\app", CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden }).WaitForExit();
                if (GlobalVariables.relE)
                    GlobalVariables.reloadElements.Invoke();
#if !DEBUG
            }
            catch { throw; }
#endif
        }
    }
}
