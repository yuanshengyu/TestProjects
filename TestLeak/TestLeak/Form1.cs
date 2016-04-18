using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace TestLeak
{
    public partial class Form1 : Form
    {
        private Fool fool;
        private FoolBrother brother;
        public Form1()
        {
            InitializeComponent();
            fool = new Fool();
            brother = new FoolBrother();
            brother.YoungFool = fool;
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            var i = 10;
            fool.AllocalHugeMem();
        }

        private void buttonWrong_Click(object sender, EventArgs e)
        {
            fool = null;
            GCRelease();
        }

        private void buttonRight_Click(object sender, EventArgs e)
        {
            fool = null;
            brother = null;
            GCRelease();
        }
        private void GCRelease()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
    public class Fool
    {
        private IList<byte[]> list = new List<byte[]>();
        public void AllocalHugeMem()
        {
            var buffer = new byte[10*1024*1024];
            for(int i=0;i<buffer.Length;i++){
                buffer[i] = 1;
            }
            list.Add(buffer);
        }
    }
    public class FoolBrother
    {
        public Fool YoungFool { get; set; }
    }
}
