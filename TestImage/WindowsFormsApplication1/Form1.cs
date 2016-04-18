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
using System.Runtime.Serialization.Formatters.Binary;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            pictureBox1.Image = GetImageFromFile("D:\\4.jpg");
        }
        public static Image GetImageFromFile(string filename)
        {
            Image image = Image.FromFile(filename);
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, image);
            ms.Seek(0, SeekOrigin.Begin);
            image.Dispose();
            return new BinaryFormatter().Deserialize(ms) as Image;
        }
    }
}
