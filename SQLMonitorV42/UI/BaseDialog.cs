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
    public partial class BaseDialog : Form
    {
        public BaseDialog()
        {
            InitializeComponent();
            this.Icon = Icon.FromHandle(Resources.Server2.GetHicon());
        }

        public string Description
        {
            set { lblDescription.Text = value; }
        }
    }
}
