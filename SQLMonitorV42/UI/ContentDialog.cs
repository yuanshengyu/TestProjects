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
    public partial class ContentDialog : BaseDialog
    {
        private bool isLoading = true;

        public ContentDialog()
        {
            InitializeComponent();
        }

        public ContentDialog(string Content, bool IsCaseSenstive, bool IsObject, List<string> Items)
            : this()
        {
            txtContent.Text = Content;
            chkCaseSenstive.Checked = IsCaseSenstive;
            if (IsObject)
                rbSearchTypeObject.Checked = true;
            else
                rbSearchTypeContent.Checked = true;
            if (Items.Count == 0 && Content != null)
                Items.Add(Content);
            Items.Where(f => f != null).Reverse().ForEach(i => cboHistories.Items.Add(i));
            if (cboHistories.Items.Count > 0)
                cboHistories.SelectedIndex = 0;
            isLoading = false;
            txtContent.Focus();
        }

        public string Content
        {
            get { return txtContent.Text; }
        }

        public bool IsObject
        {
            get { return rbSearchTypeObject.Checked; }
        }

        public bool IsCaseSenstive
        {
            get { return chkCaseSenstive.Checked; }
        }

        private void OnGoClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtContent.Text))
            {
                this.DialogResult = DialogResult.OK;
            }
            else
                epHint.SetError(txtContent, "Please input content to search.");
        }

        private void OnHistoriesSelectedIndexChanged(object sender, EventArgs e)
        {
            if (!isLoading)
                txtContent.Text = cboHistories.SelectedItem.ToString();
        }

        private void OnContentDragDrop(object sender, DragEventArgs e)
        {
            Utils.SetDragDropContent(txtContent, e);
        }

        private void OnContentDragEnter(object sender, DragEventArgs e)
        {
            Utils.HandleContentDragEnter(e);
        }
    }
}
