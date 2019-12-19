﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Drawing;
using System.Linq;
using System.Xml;
using System.IO.Compression;

namespace UpTool2
{
    static class Program
    {
        public static Form splash;
        public static bool online;
        [STAThread]
        static void Main()
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
                    if (!Directory.Exists(GlobalVariables.dir + @"\Apps"))
                        Directory.CreateDirectory(GlobalVariables.dir + @"\Apps");
                    string xml = GlobalVariables.dir + @"\info.xml";
                    FixXML(xml);
                    string metaXML = XDocument.Load(xml).Element("meta").Element("UpdateSource").Value;
                    online = Ping(metaXML);
                    if (!online || UpdateCheck(GlobalVariables.dir, metaXML))
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

        static void ShowSplash()
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

        static void FixXML(string xml)
        {
            try
            {
                if ((!File.Exists(xml)) || XDocument.Load(xml).Element("meta") == null)
                    new XElement("meta", new XElement("UpdateSource", "https://raw.githubusercontent.com/JFronny/UpTool2/master/Meta.xml"), new XElement("Repos", new XElement("Repo", new XElement("Name", "UpTool2 official Repo"), new XElement("Link", "https://raw.githubusercontent.com/JFronny/UpTool2/master/Repo.xml"))), new XElement("LocalRepo")).Save(xml);
                else
                {
                    XDocument x = XDocument.Load(xml);
                    XElement meta = x.Element("meta");
                    if (XDocument.Load(xml).Element("meta").Element("UpdateSource") == null
                        || XDocument.Load(xml).Element("meta").Element("UpdateSource").Value == null
                        || XDocument.Load(xml).Element("meta").Element("UpdateSource").Value == "https://raw.githubusercontent.com/CreepyCrafter24/UpTool2/master/Meta.xml")
                        meta.Add(new XElement("UpdateSource", "https://raw.githubusercontent.com/JFronny/UpTool2/master/Meta.xml"));
                    if (XDocument.Load(xml).Element("meta").Element("Repos") == null
                        || XDocument.Load(xml).Element("meta").Element("Repos").Value == null
                        || XDocument.Load(xml).Element("meta").Element("Repos").Value == "https://github.com/CreepyCrafter24/UpTool2/releases/download/Repo/Repo.xml")
                        meta.Add(new XElement("Repos", new XElement("Repo", new XElement("Name", "UpTool2 official Repo"), new XElement("Link", "https://raw.githubusercontent.com/JFronny/UpTool2/master/Repo.xml"))));
                    else if (XDocument.Load(xml).Element("meta").Element("Repos").Elements("Repo").Count() == 0)
                        meta.Element("Repos").Add(new XElement("Repo", new XElement("Name", "UpTool2 official Repo"), new XElement("Link", "https://raw.githubusercontent.com/JFronny/UpTool2/master/Repo.xml")));
                    else
                        meta.Element("Repos").Elements("Repo").Select(s => s.Element("Link"))
                            .Where(s => s.Value == "https://github.com/JFronny/UpTool2/releases/download/Repo/Repo.xml")
                            .ToList().ForEach(s => s.Value = "https://raw.githubusercontent.com/JFronny/UpTool2/master/Repo.xml");
                    if (XDocument.Load(xml).Element("meta").Element("LocalRepo") == null)
                        meta.Add(new XElement("LocalRepo"));
                    x.Save(xml);
                }
            }
            catch (XmlException)
            {
                new XElement("meta", new XElement("Repos", new XElement("Repo", new XElement("Name", "UpTool2 official Repo"), new XElement("Link", "https://raw.githubusercontent.com/JFronny/UpTool2/master/Repo.xml"))), new XElement("LocalRepo")).Save(xml);
            }
        }

        static bool UpdateCheck(string dir, string metaXML)
        {
            XElement meta = XDocument.Load(metaXML).Element("meta");
            if (Assembly.GetExecutingAssembly().GetName().Version < Version.Parse(meta.Element("Version").Value))
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
                if (Directory.Exists(dir + @"\Install\tmp"))
                    Directory.Delete(dir + @"\Install\tmp", true);
                Directory.CreateDirectory(dir + @"\Install\tmp");
                using (MemoryStream ms = new MemoryStream(dl))
                using (ZipArchive ar = new ZipArchive(ms))
                {
                    ar.Entries.Where(s => !string.IsNullOrEmpty(s.Name)).ToList().ForEach(s =>
                    {
                        s.ExtractToFile(dir + @"\Install\tmp" + s.Name, true);
                    });
                }
                splash.Hide();
                Process.Start(new ProcessStartInfo { FileName = "cmd.exe", Arguments = @"/C timeout /t 2 & xcopy /s /e /y tmp\* .", CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden, WorkingDirectory = dir + @"\Install" });
                return false;
            }
            return true;
        }

        public static bool Ping(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = 3000;
                request.AllowAutoRedirect = false;
                request.Method = "HEAD";

                using (var response = request.GetResponse())
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
