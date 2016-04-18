using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using log4net;
using System.IO;


namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        ILog log = null;
        public bool flag = true;
        private string IP = "";
        private int Port = 0;
        private string msg;

        Thread threadTcpReceive;
        private FileInfo finfo = new FileInfo("通信记录.txt");
        private StreamWriter sw;
        public delegate void DelShow();
        private void Show()
        {
            richTextBox1.Text += msg;
        }
        public Form1()
        {
            InitializeComponent();
            //sw = new StreamWriter(finfo.Open(FileMode.Create));
            log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            threadTcpReceive = new Thread(new ThreadStart(ReceiveJHData));
        }
        
        public void ReceiveJHData()
        {
            TcpClient Client;
            NetworkStream stream;
            while (flag)
            {//第一层循环，连接
                try
                {
                    Client = null;
                    Client = new TcpClient(IP, Port);

                    stream = null;
                    stream = Client.GetStream();
                    stream.ReadTimeout = 100;
                }
                catch
                {
                    Client = null;
                    msg = "error when client established";
                    DelShow ds = new DelShow(Show);
                    Invoke(ds);
                    continue;
                }
                int i = 0;
                while (flag)
                {
                    try
                    {
                        if (!(stream.DataAvailable))
                        {
                            Thread.Sleep(100);
                            continue;
                        }
                        byte[] responseBytes = new byte[30];
                        int Count = stream.Read(responseBytes, 0, responseBytes.Length);
                        msg = Encoding.ASCII.GetString(responseBytes).Substring(0, responseBytes.Length).Trim();
                        msg = string.Format("string({0}): " , ++i)+msg;
                        sw.WriteLine(msg);
                        msg += "\r\n";
                        DelShow ds = new DelShow(Show);
                        Invoke(ds);
                    }
                    catch
                    {
                        break;
                    }
                }
            }
        }

        private void Close()
        {
            if (flag)
            {
                flag = false;
                Thread.Sleep(1000);
                if (threadTcpReceive.IsAlive)
                {
                    try
                    {
                        threadTcpReceive.Abort();
                    }
                    catch
                    {

                    }
                }
            }
            if (sw != null)
            {
                sw.Close();
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            log.Info("start");
            IP = textBox1.Text;
            Port = Convert.ToInt32(textBox2.Text);
            sw = new StreamWriter(finfo.Open(FileMode.Create));
            threadTcpReceive.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Close();
        }
    }
}
