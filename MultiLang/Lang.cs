using System.Collections.Generic;
using System.Windows.Forms;
using LinePutScript;

namespace MultiLang
{
    /// <summary>
    /// 语言 单个Form
    /// </summary>
    public class LangForm
    {
        /// <summary>
        /// 指示当前Form名称
        /// </summary>
        public string Form;
        /// <summary>
        /// 当前Form的Text (标题)
        /// </summary>
        public string FormText;
        /// <summary>
        /// Windows From 中的文本替换
        /// </summary>
        public List<Line> LanginForm;
        /// <summary>
        /// 纯文本中的文本替换
        /// </summary>
        public List<Line> LanginText;
        /// <summary>
        /// 当前form的全部翻译数据
        /// </summary>
        public List<Line> LineforForm;
        /// <summary>
        /// 生成一个语言 针对
        /// </summary>
        /// <param name="lineforForm">读取的语言文件内容</param>
        public LangForm(List<Line> lineforForm)//form#frmlang
        {
            LineforForm = lineforForm;
            Form = LineforForm[0].Info;
            FormText = LineforForm[0].Text;
            LanginForm = LineforForm.FindAll(x => x.Name == "frm");
            LanginText = LineforForm.FindAll(x => x.Name == "txt");
        }
        /// <summary>
        /// 翻译,翻译整个WinForm
        /// </summary>
        /// <param name="form">WinForm</param>
        /// <returns></returns>
        public Form Translate(Form form)
        {
            form.Text = FormText;
            foreach (var line in LanginForm)
            {
                foreach (var Con in form.Controls.Find(line.Info, true))
                {
                    Con.Text = line.Text;
                }
            }
            //foreach (var Con in form.Controls.Find("", false))
            //{
            //    tmp = LanginForm.Find(x => x.Info == Con.Name);
            //    if (tmp != null)
            //        Con.Text = tmp.Text;
            //}
            return form;
        }
        /// <summary>
        /// 翻译,由文本进行翻译
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns></returns>
        public string Translate(string text)
        {
            Line tmp = LanginText.Find(x => x.Info == text);
            if (tmp != null)
                return tmp.Text;
            return text;
        }
        /// <summary>
        /// 查找当前翻译数据中的某一组数据
        /// </summary>
        /// <param name="GroupName">组的名字</param>
        /// <returns>数据的组</returns>
        public List<Line> FindGroupLine(string GroupName)
        {
            return LineforForm.FindAll(x => x.Name == GroupName);
        }
    }
    /// <summary>
    /// 多语言支持类
    /// </summary>
    public class Lang
    {
        /// <summary>
        /// 指示当前语言
        /// </summary>
        public string Language;
        /// <summary>
        /// Windows API 中定义的由三个字母构成的语言代码
        /// </summary>
        public string ThreeLetterWindowsLanguageName;
        /// <summary>
        /// 是否是默认的主语言
        /// </summary>
        public bool Default;
        /// <summary>
        /// 语言文件读取信息
        /// </summary>
        public LpsDocument LangFile;
        /// <summary>
        /// 多个LangForms,对应不同的类
        /// </summary>
        public List<LangForm> LangForms = new List<LangForm>();
        /// <summary>
        /// 纯文本中的文本替换/或者数据组(集体)
        /// </summary>
        public List<Line> LanginText = new List<Line>();
        /// <summary>
        /// 从文件生成语言翻译
        /// </summary>
        /// <param name="langFile"></param>
        /// <param name="SoftwareName"></param>
        public Lang(string langFile, string SoftwareName)
        {
            LangFile = new LpsDocument(langFile);
            if (LangFile.Read().Name != "MLS" || LangFile.Read().Find("lang") == null || LangFile.Read().Find("WIN") == null || LangFile.Read().Info != SoftwareName)
            {
                Language = "ERROR:FalseLangFile";
                return;
            }
            Language = LangFile.Read().Find("lang").Info;
            ThreeLetterWindowsLanguageName = LangFile.Read().Find("WIN").Info;
            Default = LangFile.ReadNext().Find("def") != null;
            bool HaveForm = false;//如果有form,就继续读
            //加载全局文本
            while (LangFile.ReadCanNext())
            {
                if (LangFile.Read().Name == "form")//加载局部文本
                {
                    HaveForm = true;
                    break;
                }
                LanginText.Add(LangFile.ReadNext());
            }

            if (HaveForm)
            {
                //先导入第一个
                List<Line> lineforForm = new List<Line>();
                lineforForm.Add(LangFile.ReadNext());

                //加载局部文本
                while (LangFile.ReadCanNext())
                {
                    if (LangFile.Read().Name == "form")
                    {
                        LangForms.Add(new LangForm(lineforForm));
                        lineforForm = new List<Line>();
                        lineforForm.Add(LangFile.ReadNext());
                    }
                    lineforForm.Add(LangFile.ReadNext());
                }
                //结束手动收尾
                LangForms.Add(new LangForm(lineforForm));
            }
        }
        /// <summary>
        /// 生成一个空的语言文件,对应Null
        /// </summary>
        public Lang()
        {
            Language = "null";
            ThreeLetterWindowsLanguageName = "NUL";
        }
        /// <summary>
        /// 翻译,翻译整个WinForm
        /// </summary>
        /// <param name="form">WinForm</param>
        public void Translate(Form form)
        {
            if (Language == "null")//如果为语言为null,直接退回原文本
                return;
            LangForm tmp = LangForms.Find(x => x.Form == form.Name);
            if (tmp != null)
                tmp.Translate(form);
        }
        /// <summary>
        /// 翻译,获得对应Form的指定文本
        /// </summary>
        /// <param name="name">指定名称</param>
        /// <param name="form">WinForm</param>
        /// <returns>返回翻译过后的文本,或者原值</returns>
        public string Translate(Form form, string name)
        {
            if (Language == "null")//如果为语言为null,直接退回原文本
                return name;
            LangForm tmp = LangForms.Find(x => x.Form == form.Name);//先在Form中找
            if (tmp != null)
            {
                string stmp = tmp.Translate(name);
                if (stmp != name)
                    return stmp;
            }
            //如果没有,就在本地找
            Line ltmp = LanginText.Find(x => x.Info == name);
            if (ltmp != null)
                return ltmp.Text;
            return name;
        }
        /// <summary>
        /// 翻译 查找指定名称,退回翻译后的文本
        /// </summary>
        /// <param name="name">翻译名称</param>
        /// <returns></returns>
        public string Translate(string name)
        {
            if (Language == "null")//如果为语言为null,直接退回原文本
                return name;
            Line ltmp = LanginText.Find(x => x.Info == name);
            if (ltmp != null)
                return ltmp.Text;
            return name;
        }
        /// <summary>
        /// 翻译 查找指定名称,退回翻译后的文本
        /// </summary>
        /// <param name="name">翻译名称</param>
        /// <param name="replace">替换[int]中的文本,按先后顺序</param>
        /// <returns></returns>
        public string Translate(string name, params string[] replace)
        {
            string tans = Translate(name);
            for (int i = 0; i < replace.Length; i++)
            {
                tans = tans.Replace($"[{i}]", replace[i]);
            }
            return tans;
        }
        /// <summary>
        /// 获取相应的LangForm
        /// </summary>
        /// <param name="form">要找的form</param>
        /// <returns>LangForm,如果没找着返回null</returns>
        public LangForm FindLangForm(Form form)
        {
            return LangForms.Find(x => x.Form == form.Name);
        }
        /// <summary>
        /// 查找当前翻译数据中的某一组数据
        /// </summary>
        /// <param name="GroupName">组的名字</param>
        /// <returns>数据的组</returns>
        public List<Line> FindGroupLine(string GroupName)
        {
            return LanginText.FindAll(x => x.Name == GroupName);
        }
    }
}
