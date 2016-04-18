using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace TestDrop
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        //拖入后处理
        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] names = (string[])e.Data.GetData(DataFormats.FileDrop);
            FileStream fs = new FileStream(names[0], FileMode.Open, FileAccess.ReadWrite);
            StreamReader sr = new StreamReader(fs,Encoding.Default);
            while (true)
            {
                string line = sr.ReadLine();
                if (line == null)
                {
                    break;
                }
                richTextBox1.AppendText(line + System.Environment.NewLine);
            }
        }

        //拖入时
        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            bool result = e.Data.GetDataPresent(DataFormats.FileDrop);
            if (!result)
            {
                e.Effect = DragDropEffects.None;
                return;
            }
            string[] names = (string[])e.Data.GetData(DataFormats.FileDrop);
            FileInfo fi = new FileInfo(names[0]);
            if (fi.Extension == @".txt")
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
    }
}
