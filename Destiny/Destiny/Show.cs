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
    public partial class Show : Form
    {
        public Show()
        {
            InitializeComponent();
        }
        public Show(Future future, string type):this()
        {
            richLong.Text = future.LongCharge;
            labelType.Text = type;
            if (type == "事业")
            {
                richWhat.Text = future.Cause;
            }
            else if (type == "经商")
            {
                richWhat.Text = future.Cause; richWhat.Text = future.Business;
            }
            else if (type == "求名")
            {
                richWhat.Text = future.Fame;
            }
            else if (type == "外出")
            {
                richWhat.Text = future.GoOut;
            }
            else if (type == "婚恋")
            {
                richWhat.Text = future.Love;
            }
            else if (type == "决策")
            {
                richWhat.Text = future.Decision;
            }
        }
    }
}
