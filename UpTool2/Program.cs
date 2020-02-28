﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using UpTool2.Tool;
using Shortcut = UpTool2.Tool.Shortcut;

namespace UpTool2
{
    internal static class Program
    {
        public static Form splash;
        public static bool online;

        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ShowSplash();
            string appGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value.ToString();
            string mutexId = string.Format("Global\\{{{0}}}", appGuid);
            var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);
            using (var mutex = new Mutex(false, mutexId, out bool createdNew, securitySettings))
            {
                var hasHandle = false;
#if !DEBUG
                try
                {
#endif
                try
                {
                    hasHandle = mutex.WaitOne(5000, false);
                    if (hasHandle == false)
                        throw new TimeoutException("Timeout waiting for exclusive access");
                }
                catch (AbandonedMutexException)
                {
#if DEBUG
                    Console.WriteLine("Mutex abandoned");
#endif
                    hasHandle = true;
                }
                if (!Directory.Exists(PathTool.dir))
                    Directory.CreateDirectory(PathTool.dir);
                FixXML();
                string metaXML = XDocument.Load(PathTool.infoXML).Element("meta").Element("UpdateSource").Value;
                online = Ping(metaXML);

#if !DEBUG
                    if (Application.ExecutablePath != PathTool.GetProgPath("Install", "UpTool2.exe"))
                    {
                        if ((!online))
                            throw new WebException("Could fetch Metadata (are you online?)");
                        if (MessageBox.Show(@"Thank you for downloading UpTool2.
To prevent inconsistent behavior you will need to install this before running.
Files will be placed in %appdata%\UpTool2 and %appdata%\Microsoft\Windows\Start Menu\Programs
Do you want to continue?", "UpTool2", MessageBoxButtons.YesNo) != DialogResult.Yes)
                            throw new Exception("Exiting...");
                        MessageBox.Show("Installing an Update. Please restart from your start menu!");
                        installUpdate(XDocument.Load(metaXML).Element("meta"));
                        Shortcut.Make(PathTool.GetProgPath("Install", "UpTool2.exe"), Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "UpTool2.lnk"));
                        mutex.ReleaseMutex();
                        Environment.Exit(0);
                    }
                    if (!File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "UpTool2.lnk")))
                        Shortcut.Make(PathTool.GetProgPath("Install", "UpTool2.exe"), Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "UpTool2.lnk"));
#endif
                if (!Directory.Exists(PathTool.GetProgPath("Apps")))
                    Directory.CreateDirectory(PathTool.GetProgPath("Apps"));
                if (!online || UpdateCheck(metaXML))
                    Application.Run(new MainForm());
#if !DEBUG
                }
                catch (Exception e1)
                {
                    MessageBox.Show(e1.ToString());
                }
                finally
                {
                    if (hasHandle)
                        mutex.ReleaseMutex();
                }
#endif
            }
        }

        private static void ShowSplash()
        {
            splash = new Form
            {
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.None,
                ControlBox = false,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowIcon = false,
                ShowInTaskbar = false,
                Size = new Size(700, 400),
                ForeColor = Color.Green,
                TopMost = true
            };
            splash.MaximumSize = splash.Size;
            splash.MinimumSize = splash.Size;
            Label splashL = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Loading",
                Font = new Font(FontFamily.GenericSansSerif, 40)
            };
            splash.Controls.Add(splashL);
            splash.Show();
            splash.BringToFront();
        }

        public static void FixXML(bool throwOnError = false)
        {
            try
            {
                if ((!File.Exists(PathTool.infoXML)) || XDocument.Load(PathTool.infoXML).Element("meta") == null)
                    new XElement("meta").Save(PathTool.infoXML);
                XDocument x = XDocument.Load(PathTool.infoXML);
                XElement meta = x.Element("meta");
                if (meta.Element("UpdateSource") == null)
                    meta.Add(new XElement("UpdateSource"));
                if (new string[] { "", "https://raw.githubusercontent.com/CreepyCrafter24/UpTool2/master/Meta.xml",
                        "https://raw.githubusercontent.com/JFronny/UpTool2/master/Meta.xml" }
                    .Contains(meta.Element("UpdateSource").Value))
                    meta.Element("UpdateSource").Value = "https://gist.githubusercontent.com/JFronny/f1ccbba3d8a2f5862592bb29fdb612c4/raw/Meta.xml";
                if (meta.Element("Repos") == null)
                    meta.Add(new XElement("Repos"));
                if (meta.Element("Repos").Elements("Repo").Count() == 0)
                    meta.Element("Repos").Add(new XElement("Repo", new XElement("Name", "UpTool2 official Repo"), new XElement("Link", "https://gist.githubusercontent.com/JFronny/f1ccbba3d8a2f5862592bb29fdb612c4/raw/Repo.xml")));
                meta.Element("Repos").Elements("Repo").Select(s => s.Element("Link"))
                    .Where(s => new string[] { null, "https://github.com/JFronny/UpTool2/releases/download/Repo/Repo.xml",
                        "https://raw.githubusercontent.com/JFronny/UpTool2/master/Repo.xml"}.Contains(s.Value))
                    .ToList().ForEach(s => s.Value = "https://gist.githubusercontent.com/JFronny/f1ccbba3d8a2f5862592bb29fdb612c4/raw/Repo.xml");
                if (meta.Element("LocalRepo") == null)
                    meta.Add(new XElement("LocalRepo"));
                x.Save(PathTool.infoXML);
            }
            catch (XmlException)
            {
                if (throwOnError)
                    throw;
                else
                {
                    MessageBox.Show("Something went wrong while trying to parse XML. Retrying...");
                    File.Delete(PathTool.infoXML);
                    FixXML();
                }
            }
        }

        private static bool UpdateCheck(string metaXML)
        {
            XElement meta = XDocument.Load(metaXML).Element("meta");
            if (Assembly.GetExecutingAssembly().GetName().Version < Version.Parse(meta.Element("Version").Value))
            {
                installUpdate(meta);
                return false;
            }
            return true;
        }

        private static void installUpdate(XElement meta)
        {
            byte[] dl;
            using (DownloadDialog dlg = new DownloadDialog(meta.Element("File").Value))
            {
                if (dlg.ShowDialog() != DialogResult.OK)
                    throw new Exception("Failed to update");
                dl = dlg.result;
            }
            using (SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider())
            {
                string pkghash = BitConverter.ToString(sha256.ComputeHash(dl)).Replace("-", string.Empty).ToUpper();
                if (pkghash != meta.Element("Hash").Value.ToUpper())
                    throw new Exception("The hash is not equal to the one stored in the repo:\r\nPackage: " + pkghash + "\r\nOnline: " + meta.Element("Hash").Value.ToUpper());
            }

            if (Directory.Exists(PathTool.GetProgPath("Install", "tmp")))
                Directory.Delete(PathTool.GetProgPath("Install", "tmp"), true);
            Directory.CreateDirectory(PathTool.GetProgPath("Install", "tmp"));
            using (MemoryStream ms = new MemoryStream(dl))
            using (ZipArchive ar = new ZipArchive(ms))
            {
                ar.Entries.Where(s => !string.IsNullOrEmpty(s.Name)).ToList().ForEach(s =>
                {
                    s.ExtractToFile(PathTool.GetProgPath("Install", "tmp", s.Name), true);
                });
            }
            splash.Hide();
            Process.Start(new ProcessStartInfo { FileName = "cmd.exe", Arguments = @"/C timeout /t 2 & xcopy /s /e /y tmp\* .", CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden, WorkingDirectory = PathTool.GetProgPath("Install") });
        }

        public static bool Ping(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = 3000;
                request.AllowAutoRedirect = false;
                request.Method = "HEAD";
                using var response = request.GetResponse();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}