﻿using System.Collections.Generic;
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
        /// <param name="langFile">读取的语言文件内容</param>
        public LangForm(List<Line> lineforForm)//form#frmlang
        {
            LineforForm = lineforForm;
            Form = LineforForm[0].Info;
            FormText = LineforForm[0].Text;
            LanginForm = LineforForm.FindAll(x => x.Name == "frm");
            LanginText = LineforForm.FindAll(x => x.Name == "txt");
        }
        /// <summary>
        /// 翻译，翻译整个WinForm
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
        /// 翻译，由文本进行翻译
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

    public class Lang
    {
        /// <summary>
        /// 指示当前语言
        /// </summary>
        public string Language;
        /// <summary>
        /// 是否是默认的主语言
        /// </summary>
        public bool Default;
        /// <summary>
        /// 语言文件读取信息
        /// </summary>
        public LpsDocument LangFile;
        public List<LangForm> LangForms = new List<LangForm>();
        /// <summary>
        /// 纯文本中的文本替换/或者数据组(集体)
        /// </summary>
        public List<Line> LanginText = new List<Line>();

        public Lang(string langFile, string SoftwareName)
        {
            LangFile = new LpsDocument(langFile);
            if (LangFile.Read().Name != "MLS" || LangFile.Read().Find("lang") == null || LangFile.Read().Info != SoftwareName)
            {
                Language = "ERROR:FalseLangFile";
                return;
            }
            Language = LangFile.Read().Find("lang").Info;
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
        /// 翻译，翻译整个WinForm
        /// </summary>
        /// <param name="form">WinForm</param>
        /// <returns>返回翻译过后的form</returns>
        public Form Translate(Form form)
        {
            LangForm tmp = LangForms.Find(x => x.Form == form.Name);
            if (tmp != null)
                return tmp.Translate(form);
            return form;
        }
        /// <summary>
        /// 翻译，由文本进行翻译
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>返回翻译过后的文本，或者原值</returns>
        public string Translate(string text, Form form)
        {
            LangForm tmp = LangForms.Find(x => x.Form == form.Name);//先在Form中找
            if (tmp != null)
            {
                string stmp = tmp.Translate(text);
                if (stmp != text)
                    return stmp;
            }
            //如果没有，就在本地找
            Line ltmp = LanginText.Find(x => x.Info == text);
            if (ltmp != null)
                return ltmp.Text;
            return text;
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
