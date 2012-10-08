using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;

namespace FixRes
{
    public partial class FixResForm : Form
    {
        public FixResForm()
        {
            InitializeComponent();
        }

        #region UI

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            if (openFileDialog1.FileName != null)
            {
                txtResPath.Text = openFileDialog1.FileName;
                ValidateFile();
            }
        }

        private void txtResPath_TextChanged(object sender, EventArgs e)
        {
            ValidateFile();
        }


        private void btnPreview_Click(object sender, EventArgs e)
        {
            PreviewConvert();

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (PreviewConvert())
            {
                try
                {
                    File.WriteAllText(txtResPath.Text, txtContent.Text);
                    lblError.Visible = false;
                    MessageBox.Show("File saved");
                }
                catch (Exception x)
                {
                    lblError.Text = "Could not write file: " + x.Message;
                    lblError.Visible = true;
                }
            }
        }

        #endregion

        #region Logic

        private void ValidateFile()
        {
            String fileName = txtResPath.Text;
            btnPreview.Enabled = false;
            btnSave.Enabled = false;
            if (File.Exists(fileName))
            {
                try
                {
                    txtContent.Text = File.ReadAllText(fileName);
                    lblError.Visible = false;
                    btnPreview.Enabled = true;
                    btnSave.Enabled = true;
                }
                catch (Exception x)
                {
                    lblError.Text = "File cannot be read: " + x.Message;
                    lblError.Visible = true;
                }
            }
            else
            {
                lblError.Text = "File does not exist";
                lblError.Visible = true;
                txtContent.Text = "";
            }
        }

        private bool PreviewConvert()
        {
            try
            {
                txtContent.Text = FixResource(txtResPath.Text);
                lblError.Visible = false;
                return true;
            }
            catch (Exception x)
            {
                lblError.Text = "Could not convert file: " + x.Message;
                lblError.Visible = true;
            }
            return false;
        }

        private string FixResource(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            Regex rxName = new Regex(@".*\.(.*)\.DisplayName$");

            foreach (XmlNode node in doc.SelectNodes("//root/data/value"))
            {
                if (String.IsNullOrEmpty(node.InnerText) || node.InnerText == "Sage.Platform.Orm.Entities.OrmFieldProperty")
                {
                    String name = ((XmlElement)node.ParentNode).GetAttribute("name");
                    Match m = rxName.Match(name);
                    if (m.Success)
                    {
                        node.InnerText = m.Groups[1].Value;
                    }
                }
            }
            using (StringWriter buf = new StringWriter())
            {
                XmlWriterSettings settings = new XmlWriterSettings { Indent = true };
                using (XmlWriter w = XmlWriter.Create(buf, settings))
                {
                    doc.WriteTo(w);
                }
                return buf.GetStringBuilder().ToString();
            }
        }

        #endregion

    }
}
