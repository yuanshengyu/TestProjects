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
    public partial class ForWhat : Form
    {
        public ForWhat()
        {
            InitializeComponent();
        }
        public string GetForWhat()
        {
            if (radioCause.Checked)
            {
                return "事业";
            }
            if (radioBusiness.Checked)
            {
                return "经商";
            }
            if (radioFame.Checked)
            {
                return "求名";
            }
            if (radioGoOut.Checked)
            {
                return "外出";
            }
            if (radioLove.Checked)
            {
                return "婚恋";
            }
            return "决策";
        }
    }
}
