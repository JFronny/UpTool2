using System;
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
using System.Collections.Generic;

namespace UpTool2
{
    static class Program
    {
        public static Form splash;
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
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
            string appGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value.ToString();
            string mutexId = string.Format("Global\\{{{0}}}", appGuid);
            bool createdNew;
            var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);
            using (var mutex = new Mutex(false, mutexId, out createdNew, securitySettings))
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
                    string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\UpTool2";
                    if (!Directory.Exists(dir + @"\Apps"))
                        Directory.CreateDirectory(dir + @"\Apps");
                    string xml = dir + @"\info.xml";
                    string metaXml = "https://raw.githubusercontent.com/CreepyCrafter24/UpTool2/master/Meta.xml";
                    if ((!File.Exists(xml)) || XDocument.Load(xml).Element("meta") == null)
                        new XElement("meta", new XElement("Version", 0), new XElement("Repos", new XElement("Repo", new XElement("Name", "UpTool2 official Repo"), new XElement("Link", "https://raw.githubusercontent.com/CreepyCrafter24/UpTool2/master/Repo.xml"))), new XElement("LocalRepo")).Save(xml);
                    else
                    {
                        XDocument x = XDocument.Load(xml);
                        XElement meta = x.Element("meta");
                        if (XDocument.Load(xml).Element("meta").Element("Repos") == null || XDocument.Load(xml).Element("meta").Element("Repos").Elements("Repo").Count() == 0)
                        {
                            meta.Add(new XElement("Repos", new XElement("Repo", new XElement("Name", "UpTool2 official Repo"), new XElement("Link", "https://raw.githubusercontent.com/CreepyCrafter24/UpTool2/master/Repo.xml"))));
                            meta.Add(new XElement("LocalRepo"));
                        }
                        else
                        {
                            XElement repos = meta.Element("Repos");
                            IEnumerable<XElement> reposa = repos.Elements("Repo");
                            reposa.Select(s => s.Element("Link")).Where(s => s.Value == "https://github.com/CreepyCrafter24/UpTool2/releases/download/Repo/Repo.xml").ToList().ForEach(s => s.Value = "https://raw.githubusercontent.com/CreepyCrafter24/UpTool2/master/Repo.xml");
                        }
                        x.Save(xml);
                    }
                    online = Ping(metaXml);
                    if (online)
                    {
                        XElement meta = XDocument.Load(metaXml).Element("meta");
                        int version = int.Parse(meta.Element("Version").Value);
                        if (int.Parse(XDocument.Load(xml).Element("meta").Element("Version").Value) < version)
                        {
                            if (new DownloadDialog(meta.Element("File").Value, dir + @"\update.exe").ShowDialog() != DialogResult.OK)
                                throw new Exception("Failed to update");
                            using (SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider())
                            {
                                string pkghash = BitConverter.ToString(sha256.ComputeHash(File.ReadAllBytes(dir + @"\update.exe"))).Replace("-", string.Empty).ToUpper();
                                if (pkghash != meta.Element("Hash").Value.ToUpper())
                                    throw new Exception("The hash is not equal to the one stored in the repo:\r\nPackage: " + pkghash + "\r\nOnline: " + meta.Element("Hash").Value.ToUpper());
                            }
                            new XElement("meta", new XElement("Version", version)).Save(xml);
                            splash.Hide();
                            Process.Start(new ProcessStartInfo { FileName = "cmd.exe", Arguments = "/C timeout /t 2 & copy /b/v/y \"" + dir + @"\update.exe" + "\" \"" + Application.ExecutablePath + "\" & \"" + Application.ExecutablePath + "\"", CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden });
                        }
                        else
                            Application.Run(new MainForm());
                    }
                    else
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
        public static bool online;
    }
}
