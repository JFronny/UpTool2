using System.Windows.Forms;
using System.Xml.Linq;
using UpToolLib.Tool;

namespace UpTool2
{
    public partial class SettingsForms : Form
    {
        private readonly XDocument doc;
        private readonly XElement meta;
        private readonly XElement repos;

        public SettingsForms()
        {
            InitializeComponent();
            Program.FixXml();
            doc = XDocument.Load(PathTool.InfoXml);
            meta = doc.Element("meta");
            repos = meta.Element("Repos");
            foreach (XElement repo in repos.Elements("Repo"))
                sourceGrid.Rows.Add(repo.Element("Name").Value, repo.Element("Link").Value);
        }

        private void SettingsForms_FormClosing(object sender, FormClosingEventArgs e)
        {
            repos.RemoveNodes();
            for (int y = 0; y < sourceGrid.Rows.Count; y++)
                if (y + 1 < sourceGrid.Rows.Count)
                    repos.Add(new XElement("Repo", new XElement("Name", (string) sourceGrid.Rows[y].Cells[0].Value),
                        new XElement("Link", (string) sourceGrid.Rows[y].Cells[1].Value)));
            doc.Save(PathTool.InfoXml);
        }
    }
}