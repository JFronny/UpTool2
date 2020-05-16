using System;
using System.Windows.Forms;
using UpToolLib;

namespace Installer
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            try
            {
                MutexLock.Lock();
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new InstallerForm());
            }
            finally
            {
                MutexLock.Unlock();
            }
        }
    }
}