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
using System.IO;
using System.Text.RegularExpressions;


namespace TestTCP
{
    public partial class FrmMain : Form
    {
        private bool connected = false;
        private byte[] orders = null;
        public bool flag = true;
        private string IP = "";
        private int Port = 0;
        private int interval = 10;
        private string msg;
        private FileInfo finfo;
        private StreamWriter sw;
        public delegate void DelShow();
        public Thread threadTcpReceive=null;
        TcpClient Client = null;
        NetworkStream stream = null;
        private void Show()
        {
            txtReceive.AppendText(msg);
        }
        public FrmMain()
        {
            InitializeComponent();
            ChangeEnabledByConnected(false);
            status.Text = "未连接";
        }
        private void ChangeEnabledByConnected(bool connected)
        {
            if (!connected)
            {
                txtIP.Enabled = true;
                txtPort.Enabled = true;
                txtSend.Enabled = true;
                btnCycle.Enabled = false;
                btnOnce.Enabled = false;
            }
            else
            {
                txtIP.Enabled = false;
                txtPort.Enabled = false;
                btnCycle.Enabled = true;
                btnOnce.Enabled = true;
            }
        }
        private void ChangeEnabledByCycle(bool cycle)
        {
            if (cycle)
            {
                btnCycle.Enabled = true;
                btnOnce.Enabled = false;
            }
            else
            {
                btnCycle.Enabled = false;
                btnOnce.Enabled = true;
            }
        }
        private void Record()
        {
            try
            {
                string head = string.Format("({0}): ", DateTime.Now.ToLongTimeString());
                msg = head + msg;
                sw.WriteLine(msg);
                msg += "\r\n";
                DelShow ds = new DelShow(Show);
                Invoke(ds);
            }
            catch { }
        }
        private void Record2()
        {
            string head = string.Format("({0}): ", DateTime.Now.ToLongTimeString());
            msg = head + msg;
            sw.WriteLine(msg);
        }
        public void CycleRead()
        {
            while (true)
            {//第一层循环，连接
                try
                {
                    if (Client != null)
                        Client.Close();
                    Client = null;
                    Client = new TcpClient(IP, Port);

                    stream = null;
                    stream = Client.GetStream();
                    //stream.ReadTimeout = 1000;//90s//注意在退出的时候，有可能线程处于Read阻塞状态
                }
                catch(Exception e)
                {
                    Client = null;
                    msg = "连接错误: "+e.Message;
                    Record();
                    continue;
                }
                while (true)
                {
                    try
                    {
                        ReadDatas();
                        Thread.Sleep(interval);
                    }
                    catch(Exception e)
                    {
                        msg = e.Message;
                        Record();
                        break;
                    }
                }
            }
            if (Client != null)
            {
                Client.Close();
                Client = null;
            }
        }
        private byte[] GetBytes(string str)
        {
            string[] strs = str.Split(' ');
            List<byte> lbytes = new List<byte>();
            foreach (string s in strs)
            {
                if (s.Trim() != "")
                {
                    try
                    {
                        byte b = Convert.ToByte(s.Trim());
                        lbytes.Add(b);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            return lbytes.ToArray();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (btnCycle.Text == "停止")
            {
                btnCycle_Click(btnCycle, new EventArgs());
                btnConnect_Click(btnConnect, new EventArgs());
            }
        }

        private void btnCycle_Click(object sender, EventArgs e)
        {
            if (btnCycle.Text == "循环读取")
            {
                orders = GetBytes(txtSend.Text.Trim());
                interval = (int)(numericInterval.Value) * 1000;
                btnCycle.Text = "停止";
                ThreadStart start = new ThreadStart(CycleRead);
                threadTcpReceive = new Thread(start);
                threadTcpReceive.Start();
                btnConnect.Enabled = false;
            }
            else
            {
                btnCycle.Text = "循环读取";
                Thread.Sleep(100);
                threadTcpReceive.Abort();
                btnConnect.Enabled = true;
            }
        }
        private string BytesToString(byte[] bytes)
        {
            bool hex = checkBoxHex.Checked;
            string str = string.Empty;
            foreach (byte b in bytes)
            {
                if (hex)
                    str += b.ToString("X2") + " ";
                else
                    str += b.ToString() + " ";
            }
            return str;
        }

        private void ReadDatas()
        {
            if (orders == null || orders.Length==0)
            {
                msg = "发送命令为空";
                return;
            }
            stream.Write(orders, 0, orders.Length);
            byte[] responseBytes = new byte[400];
            string responseString = string.Empty;
            System.Threading.Thread.Sleep(1000);//不能取消！
            try
            {
                int count = stream.Read(responseBytes, 0, responseBytes.Length);
                byte[] bytes = new byte[count];
                Array.Copy(responseBytes, 0, bytes, 0, count);
                msg = BytesToString(bytes);
                Record();
            }
            catch(Exception ex)
            {
                msg = ex.Message;
                Record();
            }
        }

        private void btnOnce_Click(object sender, EventArgs e)
        {
            orders = GetBytes(txtSend.Text.Trim());
            ReadDatas();
        }

        private bool Connect()
        {
            try
            {
                if (Client != null)
                    Client.Close();

                Client = null;
                Client = new TcpClient(IP, Port);

                stream = null;
                stream = Client.GetStream();
                return true;
            }
            catch
            {
                return false;
            }
        }
        private void DisConnect()
        {
            if (stream != null)
                stream.Close();
            if (Client != null)
                Client.Close();
            if (sw != null)
                sw.Close();
        }
        private void btnConnect_Click(object sender, EventArgs e)
        {
            IP = txtIP.Text.Trim();
            Port = Convert.ToInt32(txtPort.Text.Trim());
            connected = false;
            if (btnConnect.Text == "连接")
            {
                bool result = Connect();
                if (result)
                {
                    btnConnect.Text = "断开";
                    ChangeEnabledByConnected(true);
                    ChangeEnabledByCycle(checkBoxCycle.Checked);
                    status.Text = "已连接";
                    connected = true;

                    string str = DateTime.Now.ToLongTimeString();
                    str = str.Replace(":", "_") + " " + IP;
                    finfo = new FileInfo(str + ".txt");
                    sw = new StreamWriter(finfo.Open(FileMode.Create));
                }
                else
                    status.Text = "连接失败";
            }
            else
            {
                DisConnect();
                btnConnect.Text = "连接";
                ChangeEnabledByConnected(false);
                status.Text = "未连接";
            }
        }

        private void checkBoxCycle_CheckedChanged(object sender, EventArgs e)
        {
            if (connected)
                ChangeEnabledByCycle(checkBoxCycle.Checked);
        }

        private void 清空ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            txtReceive.Text = "";
        }

    }
}
