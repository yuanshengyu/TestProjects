using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace UnsafeCode
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ThreadStart start = new ThreadStart(Func);
            Thread st = new Thread(start);
            st.Start();
        }
        private void Func(){
            while(true){
                unsafe
                {
                    label1.Text = DateTime.Now.ToString();
                }
                Thread.Sleep(300);
            }
        }
    }
}
