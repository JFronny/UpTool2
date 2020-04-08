using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using UpToolLib.DataStructures;
using UpToolLib.Tool;

namespace UpTool2.Task
{
    class UpdateTask : IKnownAppTask
    {
        public override App App { get; }
        private readonly Action? _postInstall;
        public UpdateTask(App app, Action? postInstall = null)
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
                AppExtras.Update(App, false);
                _postInstall?.Invoke();
#if !DEBUG
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.ToString(), "Install failed");
            }
#endif
        }
    }
}
