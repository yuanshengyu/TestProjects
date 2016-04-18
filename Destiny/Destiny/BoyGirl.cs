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
    public partial class BoyGirl : Form
    {
        public BoyGirl()
        {
            InitializeComponent();
        }
        public int GetSex()
        {
            if (radioButton1.Checked)
            {
                return 1;
            }
            else if (radioButton2.Checked)
            {
                return 2;
            }
            return 3;
        }
    }
}
