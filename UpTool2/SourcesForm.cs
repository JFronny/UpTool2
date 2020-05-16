using System.Windows.Forms;
using System.Xml.Linq;
using UpToolLib.Tool;

namespace UpTool2
{
    public partial class SettingsForms : Form
    {
        private readonly XDocument _doc;
        private readonly XElement _repos;

        public SettingsForms()
        {
            InitializeComponent();
            Program.FixXml();
            _doc = XDocument.Load(PathTool.InfoXml);
            _repos = _doc.Element("meta").Element("Repos");
            sourceGrid.Columns.Clear();
            sourceGrid.Columns.Add("name", "Name");
            sourceGrid.Columns.Add("link", "Link");
            foreach (XElement repo in _repos.Elements("Repo"))
                sourceGrid.Rows.Add(repo.Element("Name").Value, repo.Element("Link").Value);
        }

        private void SettingsForms_FormClosing(object sender, FormClosingEventArgs e)
        {
            _repos.RemoveNodes();
            for (int y = 0; y < sourceGrid.Rows.Count; y++)
                if (y + 1 < sourceGrid.Rows.Count)
                    _repos.Add(new XElement("Repo", new XElement("Name", (string) sourceGrid.Rows[y].Cells[0].Value),
                        new XElement("Link", (string) sourceGrid.Rows[y].Cells[1].Value)));
            _doc.Save(PathTool.InfoXml);
        }
    }
}