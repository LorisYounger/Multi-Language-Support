using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MultiLang;
using LinePutScript;
using System.Xml;

namespace Multi_Language_Support
{
    public partial class FrmMain : Form
    {
        public List<Lang> Langs = new List<Lang>();
        public FrmMain()
        {
            InitializeComponent();
            //收集全部语言
            DirectoryInfo dis = new DirectoryInfo(Application.StartupPath);
            Lang tmp;
            StreamReader sr;
            foreach (FileInfo fi in dis.GetFiles("*.lang"))
            {
                sr = new StreamReader(fi.OpenRead(), Encoding.UTF8);
                tmp = new Lang(sr.ReadToEnd(), "Multi_Language_Support");
                sr.Close();
                sr.Dispose();
                if (!tmp.Language.Contains("ERROR"))
                {
                    Langs.Add(tmp);
                    languageToolStripMenuItem.DropDownItems.Add(Langs.Last().Language, null, LangClick);
                }
            }
            //加载语言选项
            if (Properties.Settings.Default.Lang != "")
            {
                var lang = Langs.Find(x => x.Language == Properties.Settings.Default.Lang);
                if (lang != null)
                {
                    if (!lang.Default)//判断是不是主语言，如果是，就不翻译(节约资源)
                        Translate(lang);
                }
                else
                    Properties.Settings.Default.Lang = "";
            }
        }
        /// <summary>
        /// 该Form的翻译方法
        /// </summary>
        /// <param name="lang">语言</param>
        public void Translate(Lang lang)
        {
            lang.Translate(this);
            //手动添加进行修改 例如 menu
            foreach (Line line in lang.FindLangForm(this).FindGroupLine("menu"))
                foreach (var tmp in menuStrip1.Items.Find(line.Info, true))
                {
                    tmp.Text = line.Text;
                }
            foreach (Line line in lang.FindLangForm(this).FindGroupLine(".ToolTip"))
            {
                foreach (var tmp in this.Controls.Find(line.Info.Split('.')[0], true))
                {
                    toolTip1.SetToolTip(tmp, line.Text);
                }
            }


        }
        public void LangClick(object sender, System.EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;
            Properties.Settings.Default.Lang = mi.Text;
            Properties.Settings.Default.Save();
            var lang = Langs.Find(x => x.Language == mi.Text);
            Translate(lang);
        }


        private void buttonChooseFile_Click(object sender, System.EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
                return;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(openFileDialog1.FileName);
            LpsDocument lps = new LpsDocument();
            string[] getinfo = textBoxinfo.Text.Trim('|').Split('|');
            string FormName = "";
            foreach (XmlNode tmp in xmlDoc.GetElementsByTagName("data"))
            {
                string name = ((XmlElement)tmp).GetAttribute("name");
                if (name.EndsWith(".Text") && tmp.ChildNodes[1].InnerText != "")
                {
                    lps.AddLine(new Line("frm", name.Split('.')[0], tmp.ChildNodes[1].InnerText));//可以直接拿到value
                    continue;
                }
                foreach (string get in getinfo)
                {
                    if (get != "" && name.EndsWith(get) && tmp.ChildNodes[1].InnerText != "")
                    {
                        lps.AddLine(new Line(get, name, tmp.ChildNodes[1].InnerText));//可以直接拿到value
                        continue;
                    }
                }
                if (name == ">>$this.Name")
                    FormName = tmp.ChildNodes[1].InnerText;
            }
            Line fn = lps.Assemblage.Last();
            lps.InsertLine(0, new Line("form", FormName, fn.Text));
            lps.Remove(fn);
            textBoxOutput.Text = lps.ToString().Replace("\n", "\r\n");
        }

        private void textBoxOutput_DoubleClick(object sender, System.EventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }



        LpsDocument lpsEasy;

        private void button1_Click(object sender, System.EventArgs e)
        {
            lpsEasy = new LpsDocument(textBox1.Text);
            textBox2.Text = "";
            foreach (Line li in lpsEasy.Assemblage)
            {
                textBox2.AppendText(li.text + "\r\n");
            }
        }

        private void button2_Click(object sender, System.EventArgs e)
        {
            if (lpsEasy == null)
                return;
            string[] strs = textBox2.Text.Replace("\r\n", "\n").TrimEnd('\n').Split('\n');
            if (strs.Length != lpsEasy.Assemblage.Count)
                MessageBox.Show("Error Same Line:" + (strs.Length - lpsEasy.Assemblage.Count));
            else
            {
                for (int i = 0; i < lpsEasy.Assemblage.Count; i++)
                {
                    lpsEasy.Assemblage[i].text = strs[i];
                }
                textBox1.Text = lpsEasy.ToString();
            }                
        }
    }
}
