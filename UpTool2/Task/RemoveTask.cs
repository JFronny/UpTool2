using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using UpToolLib.DataStructures;
using UpToolLib.Tool;

namespace UpTool2.Task
{
    class RemoveTask : IKnownAppTask
    {
        public override App App { get; }
        private readonly Action? _postInstall;
        public RemoveTask(App app, Action? postInstall = null)
        {
            App = app;
            _postInstall = postInstall;
        }

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
