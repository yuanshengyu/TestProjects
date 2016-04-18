using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Destiny
{
    public partial class Info : Form
    {
        public Info()
        {
            InitializeComponent();
        }
        public void SetInfo(string info)
        {
            label1.Text = info;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

        }
    }
}
