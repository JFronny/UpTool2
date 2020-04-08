using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UpTool2.Task;

namespace UpTool2
{
    internal static class TaskPreview
    {
        public static void Show(ref List<IAppTask> tasks)
        {
            using Form tmp = new Form {Size = new Size(600, 300)};
            using CheckedListBox list = new CheckedListBox {Dock = DockStyle.Fill};
            list.Items.AddRange(tasks.ToArray());
            for (int i = 0; i < tasks.Count; i++)
                list.SetItemChecked(i, true);
            tmp.Controls.Add(list);
            tmp.ShowDialog();
            tasks = list.Items.OfType<IAppTask>().Where((s, i) => list.GetItemChecked(i)).ToList();
        }
    }
}