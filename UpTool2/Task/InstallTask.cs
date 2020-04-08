using System;
using System.Windows.Forms;
using UpToolLib.DataStructures;
using UpToolLib.Tool;

namespace UpTool2.Task
{
    class InstallTask : IKnownAppTask
    {
        public override App App { get; }
        private readonly Action? _postInstall;
        public InstallTask(App app, Action? postInstall = null)
        {
            App = app;
            _postInstall = postInstall;
        }

        public override void Run()
        {
            bool trying = true;
            while (trying)
            {
#if !DEBUG
                try
                {
#endif
                    AppInstall.Install(App, true);
                    _postInstall?.Invoke();
                    trying = false;
#if !DEBUG
                }
                catch (Exception e1)
                {
                    trying = MessageBox.Show(e1.ToString(), "Install failed", MessageBoxButtons.RetryCancel) ==
                             DialogResult.Retry;
                }
#endif
            }
        }
    }
}
