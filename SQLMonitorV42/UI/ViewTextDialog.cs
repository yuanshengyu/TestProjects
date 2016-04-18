using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Xnlab.SQLMon
{
    public partial class ViewTextDialog : BaseDialog
    {
        public ViewTextDialog()
        {
            InitializeComponent();
        }
        public ViewTextDialog(string Title, string Content)
            : this()
        {
            this.Text = Title;
            rtbContent.Font = Monitor.Instance.SetFont();
            rtbContent.Text = Content;
            rtbContent.SelectionStart = rtbContent.Text.Length;
            rtbContent.ScrollToCaret();
        }
    }
}
