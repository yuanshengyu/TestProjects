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
    public partial class PerformanceDialog : Form
    {
        public PerformanceDialog()
        {
            InitializeComponent();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.Controls.Count > 0)
            {
                var performance = this.Controls[0] as Performance;
                performance.RemovePerformanceItem();
            }
        }
    }
}
