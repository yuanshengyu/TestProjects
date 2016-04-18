using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Xnlab.SQLMon.Properties;

namespace Xnlab.SQLMon
{
    public partial class FileNameDialog : BaseDialog
    {
        private bool isSave;

        public FileNameDialog()
        {
            InitializeComponent();
        }

        public FileNameDialog(string Title, bool IsSave, string Name)
            : this()
        {
            isSave = IsSave;
            this.Text = Title;
            txtName.Text = Name;
        }

        public string FilePath
        {
            get { return txtFile.Text; }
        }

        public string ObjectName
        {
            get { return txtName.Text; }
        }

        private void OnGoClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtFile.Text))
            {
                if (!string.IsNullOrEmpty(txtName.Text))
                    this.DialogResult = DialogResult.OK;
                else
                    epHint.SetError(txtFile, "Please input name.");
            }
            else
                epHint.SetError(txtFile, "Please input file.");
        }

        private void OnChooseFileClick(object sender, EventArgs e)
        {
            FileDialog dlg;
            if (isSave)
                dlg = new SaveFileDialog();
            else
                dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
                txtFile.Text = dlg.FileName;
        }
    }
}
