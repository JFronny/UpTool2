using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace UpTool2
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            string appGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value.ToString();
            string mutexId = string.Format("Global\\{{{0}}}", appGuid);
            bool createdNew;
            var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);
            using (var mutex = new Mutex(false, mutexId, out createdNew, securitySettings))
            {
                var hasHandle = false;
                try
                {
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
                    string xml = dir + @"\info.xml";
                    if (!File.Exists(xml))
                        new XElement("meta", new XElement("Version", 0)).Save(xml);
                    XElement meta = XDocument.Load("https://github.com/CreepyCrafter24/UpTool2/releases/download/Repo/Repo.xml").Element("meta");
                    int version = int.Parse(meta.Element("Version").Value);
                    if (int.Parse(XDocument.Load(xml).Element("meta").Element("Version").Value) < version)
                    {
                        using (var client = new WebClient())
                        {
                            client.DownloadFile(meta.Element("File").Value, dir + @"\update.exe");
                        }
                        SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
                        if (BitConverter.ToString(sha256.ComputeHash(File.ReadAllBytes(dir + @"\update.exe"))).Replace("-", string.Empty).ToUpper() != meta.Element("Hash").Value)
                            throw new Exception("The hash is not equal to the one stored in the repo");
                        sha256.Dispose();
                        new XElement("meta", new XElement("Version", version)).Save(xml);
                        Process.Start(new ProcessStartInfo { FileName = "cmd.exe", Arguments = "/C echo Running Update & timeout /t 4 & copy /b/v/y \"" + dir + @"\update.exe" + "\" \"" + Application.ExecutablePath + "\" & echo Done Updating, please restart & pause" });
                    }
                    else
                        Application.Run(new MainForm());
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
            }
        }
    }
}
