using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace TestSkin
{
    public partial class Form1 : Form
    {
        private string[] files = null;
        private int index = 0;
        Sunisoft.IrisSkin.SkinEngine engine;
        public Form1()
        {
            InitializeComponent();
            files = Directory.GetFiles("Skins");
            engine = new Sunisoft.IrisSkin.SkinEngine();
            engine.SkinFile = files[index];
            engine.AddControl(this);
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (index < files.Length-1)
            {
                index++;
                skinEngine1.Active = false;
                skinEngine1.SkinFile = files[index];
                skinEngine1.Active = true;
            }
            else { index = 0; }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            skinEngine1.SkinFile= files[index];
        }
    }
}
