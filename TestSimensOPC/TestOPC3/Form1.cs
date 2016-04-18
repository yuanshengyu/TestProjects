using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using OPC.Common;
using OPC.Data.Interface;
using OPC.Data;

namespace TestOPC3
{
   
    public partial class Form1 : Form
    {
        
        private OpcServer theServer;
        private OpcGroup theGroup;
        private OPCItemDef[] ItemDefs = new OPCItemDef[2];
        private int[] HandlesServer = new int[2];
        OPCItemResult[] itemResults;
        string ServerProgID = "S7200SMART.OPCServer";
        string ItemA  = "2:192.168.2.1:0201:0201,MB0,BYTE,RW";
        string ItemB = "2:192.168.2.1:0201:0201,VW4,WORD,RW";

        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void cmdConnect_Click(object sender, EventArgs e)
        {
            try
            {
                theServer = new OpcServer();
                theServer.Connect(ServerProgID);
                cmdConnect.Enabled = false;
                cmdDisconnect.Enabled = true;
                cmdAddGroup.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cmdDisconnect_Click(object sender, EventArgs e)
        {
            cmdDisconnect.Enabled = false;
            cmdAddGroup.Enabled = false;
            cmdConnect.Enabled = true;
            if (theServer != null)
            {
                int[] aE = new int[2];
                try
                {
                    theServer.Disconnect();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                theServer = null;
            }
        }

        private void cmdAddGroup_Click(object sender, EventArgs e)
        {
            theGroup = theServer.AddGroup(txtGroup.Text, false, 900);
            if (CheckGroupActive.Checked)
            {
                theGroup.SetEnable(true);
                theGroup.Active = true;
            }
            else
            {
                theGroup.SetEnable(true);
                theGroup.Active = false;
            }
            theGroup.DataChanged += new DataChangeEventHandler(TheGrp_DataChanged);
            theGroup.ReadCompleted +=new ReadCompleteEventHandler(TheGrp_ReadCompleted);
            theGroup.WriteCompleted +=new WriteCompleteEventHandler(TheGrp_WriteCompleted);
            cmdAddGroup.Enabled = false;
            cmdDisconnect.Enabled = false;
            cmdRemGroup.Enabled = true;
            cmdAddItem.Enabled = true;
        }

        private void cmdRemGroup_Click(object sender, EventArgs e)
        {
            if (theGroup != null)
            {
                int[] aE = new int[2];
                try
                {
                    theGroup.DataChanged -= new DataChangeEventHandler(TheGrp_DataChanged);
                    theGroup.ReadCompleted -= new ReadCompleteEventHandler(TheGrp_ReadCompleted);
                    theGroup.WriteCompleted -= new WriteCompleteEventHandler(TheGrp_WriteCompleted);
                    theGroup.Remove(false);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                theGroup = null;
            }
        }

        private void CheckGroupActive_CheckedChanged(object sender, EventArgs e)
        {
            if (theGroup != null)
            {
                if (CheckGroupActive.Checked)
                {
                    theGroup.SetEnable(true);
                    theGroup.Active = true;
                }
                else
                {
                    theGroup.SetEnable(true);
                    theGroup.Active = false;
                }
            }
        }

        private void cmdAddItem_Click(object sender, EventArgs e)
        {
            ItemDefs[0] = new OPCItemDef(txtItem1.Text, true, 1234, VarEnum.VT_EMPTY);
            ItemDefs[1] = new OPCItemDef(txtItem2.Text, true, 5678, VarEnum.VT_EMPTY);
            theGroup.AddItems(ItemDefs, out itemResults);
            if (itemResults[0].Error != 0 || itemResults[1].Error != 0)
            {
                MessageBox.Show("AddItems - some failed");
                cmdConnect.Enabled = false;
                cmdDisconnect.Enabled = true;
                cmdAddGroup.Enabled = false;
                cmdRemGroup.Enabled = true;
                cmdAddItem.Enabled = true;
                cmdRemItem.Enabled = false;
            }
            HandlesServer[0] = itemResults[0].HandleServer;
            HandlesServer[1] = itemResults[1].HandleServer;
            cmdAddItem.Enabled = false;
            cmdRemGroup.Enabled = false;
            cmdRemItem.Enabled = true;
            cmdWriteSync.Enabled = true;
            cmdWriteAsync.Enabled = true;
            cmdReadSync.Enabled = true;
            cmdReadAsync.Enabled = true;
        }

        private void cmdRemItem_Click(object sender, EventArgs e)
        {
            int[] aE = new int[2];
            if (theGroup != null)
            {
                theGroup.RemoveItems(HandlesServer, out aE);
            }
            cmdRemItem.Enabled = false;
            cmdWriteSync.Enabled = false;
            cmdWriteAsync.Enabled = false;
            cmdReadSync.Enabled = false;
            cmdReadAsync.Enabled = false;
            cmdAddItem.Enabled = true;
            cmdRemGroup.Enabled = true;
        }

        private void cmdWriteSync_Click(object sender, EventArgs e)
        {
            int[] arrHSrv = new int[2];
            object[] arrVal = new object[2];
            int[] arrErr = new int[2];
            arrHSrv[0] = itemResults[0].HandleServer;
            arrHSrv[1] = itemResults[1].HandleServer;
            arrVal[0] = txtWriteVal1.Text;
            arrVal[1] = txtWriteVal2.Text;
            try
            {
                theGroup.SyncWrite(arrHSrv, arrVal, out arrErr);
                if (arrErr[0] != 0)
                    MessageBox.Show("Item 1" + "FAILED");
                if (arrErr[1] != 0)
                    MessageBox.Show("Item 2" + "FAILED");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cmdReadSync_Click(object sender, EventArgs e)
        {
            int[] aE = new int[2];
            _txtReadVal_0.Text = "";
            _txtReadVal_1.Text = "";
            int[] arrHSrv = new int[2];
            OPCItemState[] arrStat = new OPCItemState[2];
            arrHSrv[0] = itemResults[0].HandleServer;
            arrHSrv[1] = itemResults[1].HandleServer;
            try
            {
                theGroup.SyncRead(OPCDATASOURCE.OPC_DS_DEVICE, arrHSrv, out arrStat);
                if (arrStat[0].Quality == 192)
                {
                    _txtReadVal_0.Text = arrStat[0].DataValue.ToString();
                    _txtReadVal_0.BackColor = Color.White;
                }
                else
                {
                    _txtReadVal_0.Text = GetQualityText(arrStat[0].Quality);
                    _txtReadVal_0.BackColor = Color.Red;
                }
                if (arrStat[1].Quality == 192)
                {
                    _txtReadVal_1.Text = arrStat[1].DataValue.ToString();
                    _txtReadVal_1.BackColor = Color.White;
                }
                else
                {
                    _txtReadVal_1.Text = GetQualityText(arrStat[1].Quality);
                    _txtReadVal_1.BackColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cmdWriteAsync_Click(object sender, EventArgs e)
        {
            int CancelID;
            int[] aE = new int[2];
            object[] ItemValues = new object[2];
            ItemValues[0] = txtWriteVal1.Text;
            ItemValues[1] = txtWriteVal2.Text;
            try
            {
                theGroup.AsyncWrite(HandlesServer, ItemValues, 55667788, out CancelID, out aE);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cmdReadAsync_Click(object sender, EventArgs e)
        {
            int CancelID;
            int[] aE = new int[2];
            try
            {
                theGroup.AsyncRead(HandlesServer, 55667788, out CancelID, out aE);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cmdExit_Click(object sender, EventArgs e)
        {
            int[] aE = new int[2];
            if (ItemDefs[0] != null || ItemDefs[1]!=null)
            {
                if (theGroup != null)
                {
                    theGroup.RemoveItems(HandlesServer, out aE);
                }
            }
            if (theGroup != null)
            {
                try
                {
                    theGroup.DataChanged -= new DataChangeEventHandler(TheGrp_DataChanged);
                    theGroup.ReadCompleted -= new ReadCompleteEventHandler(TheGrp_ReadCompleted);
                    theGroup.WriteCompleted -= new WriteCompleteEventHandler(TheGrp_WriteCompleted);
                    theGroup.Remove(false);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                theGroup = null;
            }
            if (theServer != null)
            {
                try
                {
                    theServer.Disconnect();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            this.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtItem1.Text = ItemA;
            txtItem2.Text = ItemB;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (cmdRemItem.Enabled)
                cmdRemItem_Click(cmdRemItem, new EventArgs());
            if (cmdRemGroup.Enabled)
                cmdRemGroup_Click(cmdRemGroup, new EventArgs());
            if (cmdDisconnect.Enabled)
                cmdDisconnect_Click(cmdDisconnect, new EventArgs());
        }
        private string GetQualityText(int Quality)
        {
            string text = "";
            switch (Quality)
            {
                case 0: text = "BAD";
                    break;
                case 64: text = "UNCERTAIN";
                    break;
                case 192: text = "GOOD";
                    break;
                case 8: text = "NOT_CONNECTED";
                    break;
                case 13: text = "DEVICE_FAILURE";
                    break;
                case 16: text = "SENSOR_FAILURE";
                    break;
                case 20: text = "LAST_KNOWN";
                    break;
                case 24: text = "COMM_FAILURE";
                    break;
                case 28: text = "OUT_OF_SERVICE";
                    break;
                case 132: text = "LAST_USABLE";
                    break;
                case 144: text = "SENSOR_CAL";
                    break;
                case 148: text = "EGU_EXCEEDED";
                    break;
                case 152: text = "SUB_NORMAL";
                    break;
                case 216: text = "LOCAL_OVERRIDE";
                    break;
                default: text = "UNKNOWN QUALITY";
                    break;
            }
            return text;
        }
        public void TheGrp_DataChanged(object obj, DataChangeEventArgs e)
        {
            SetText(TxtDataChange, (Convert.ToDouble(TxtDataChange.Text)+1).ToString());
            foreach(OPCItemState s in e.sts)
            {
                if(s.Error!=0)
                {
                    if(s.HandleClient == 1234)
                    {
                        MessageBox.Show("Item 1"+ " FAILED. Error Code = "+s.Error.ToString());
                    }
                    else if(s.HandleClient == 5678)
                    {
                        MessageBox.Show("Item 2"+ " FAILED. Error Code = "+s.Error.ToString());
                    }
                }
                else
                {
                    if(s.HandleClient == 1234)
                    {
                        if(s.Quality==192)
                        {
                            if(s.DataValue!=null)
                            {
                                SetText(_txtChangeVal_0, s.DataValue.ToString());
                                _txtChangeVal_0.BackColor = Color.White;
                            }
                        }
                        else
                        {
                            SetText(_txtChangeVal_0, GetQualityText(s.Quality));
                                _txtChangeVal_0.BackColor = Color.Red;
                        }
                    }
                    else if(s.HandleClient == 5678)
                    {
                        if(s.Quality==192)
                        {
                            if(s.DataValue!=null)
                            {
                                SetText(_txtChangeVal_1, s.DataValue.ToString());
                                _txtChangeVal_1.BackColor = Color.White;
                            }
                        }
                        else
                        {
                            SetText(_txtChangeVal_1, GetQualityText(s.Quality));
                                _txtChangeVal_1.BackColor = Color.Red;
                        }
                    }
                }
            }
        }

        public delegate void SetTextCallback(TextBox textBox, string text);

        private void SetText(TextBox textBox, string text)
        {
            textBox.Text = text;
            return;
            if (TxtDataChange.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                TxtDataChange.Invoke(d, new object[] { textBox, text });
            }
            else
            {
                textBox.Text = text;
            }
        }

        public void TheGrp_ReadCompleted(object source, ReadCompleteEventArgs e)
        {
            if (TxtAReadComplete.Text == "")
            {
                SetText(TxtAReadComplete, "1");
            }
            else
            {
                SetText(TxtAReadComplete, (Convert.ToDouble(TxtAReadComplete.Text) + 1).ToString());
            }
            foreach (OPCItemState s in e.sts)
            {
                if (s.Error != 0)
                {
                    if (s.HandleClient == 1234)
                    {
                        MessageBox.Show("Item 1" + " FAILED. Error Code = " + s.Error.ToString());
                    }
                    else if (s.HandleClient == 5678)
                    {
                        MessageBox.Show("Item 2" + " FAILED. Error Code = " + s.Error.ToString());
                    }
                }
                else
                {
                    if (s.HandleClient == 1234)
                    {
                        if (s.Quality == 192)
                        {
                            if (s.DataValue != null)
                            {
                                SetText(_txtReadVal_0, s.DataValue.ToString());
                                _txtReadVal_0.BackColor = Color.White;
                            }
                        }
                        else
                        {
                            SetText(_txtReadVal_0, GetQualityText(s.Quality));
                            _txtReadVal_0.BackColor = Color.Red;
                        }
                    }
                    else if (s.HandleClient == 5678)
                    {
                        if (s.Quality == 192)
                        {
                            if (s.DataValue != null)
                            {
                                SetText(_txtReadVal_1, s.DataValue.ToString());
                                _txtReadVal_1.BackColor = Color.White;
                            }
                        }
                        else
                        {
                            SetText(_txtReadVal_1, GetQualityText(s.Quality));
                            _txtReadVal_1.BackColor = Color.Red;
                        }
                    }
                }
            }
        }

        public void TheGrp_WriteCompleted(object source, WriteCompleteEventArgs e)
        {
            SetText(TxtAWriteComplete, (Convert.ToDouble(TxtAWriteComplete.Text) + 1).ToString());
            foreach(OPCWriteResult r in e.res)
            {
                if (r.Error != 0)
                {
                    if (r.HandleClient == 1234)
                    {
                        MessageBox.Show("Item 1" + " FAILED. Error Code = " + r.Error.ToString());
                    }
                    else if (r.HandleClient == 5678)
                    {
                        MessageBox.Show("Item 2" + " FAILED. Error Code = " + r.Error.ToString());
                    }
                }
            }
        }
    }
}
