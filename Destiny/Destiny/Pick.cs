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
    public delegate void PickHandler(int a);
    public partial class Pick : Form
    {
        public event PickHandler OnPick;

        private int index = 0;
        private string information = "命运之神！";
        public Pick()
        {
            InitializeComponent();
            label2.Text = "";
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            index++;
            if (index >= information.Length)
            {
                Random rd = new Random(DateTime.Now.Millisecond);
                int pick = rd.Next(1,65);//1-64
                timer1.Enabled = false;
                OnPick(pick);
            }
            else
            {
                label2.Text +=information.Substring(index,1);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled)
            {
                return;
            }
            label2.Text = "命";
            timer1.Enabled = true;
        }
    }
}
