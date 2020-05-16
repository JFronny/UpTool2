using System;
using System.Windows.Forms;
using UpToolLib.DataStructures;
using UpToolLib.Tool;

namespace UpTool2.Task
{
    internal class RemoveTask : IKnownAppTask
    {
        private readonly Action? _postInstall;

        public RemoveTask(App app, Action? postInstall = null)
        {
            App = app;
            _postInstall = postInstall;
        }

        public override App App { get; }

        public override void Run()
        {
#if !DEBUG
            try
            {
#endif
                AppExtras.Remove(App, true);
                _postInstall?.Invoke();
#if !DEBUG
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.ToString(), "Removal failed");
            }
#endif
        }
    }
}