using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Diagnostics;
using OPCAutomation;
using System.Net;
using System.Configuration;
using log4net;
using System.Xml;
using System.Reflection;
using System.IO;
using Infecon.Common.Utility;
using Infecon.CSSD.Entity;
using System.Transactions;
using System.Threading;

namespace TestOPCShinva
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            SettingFilePath = GetExePath() + "\\Config.xml";
            InitVar();
            InitMonitorDevice();

            GetLocalServer();
            textBox1.Text = strHostIP;
            textBox2.Text = strHostSeverName;
        }
        #region 私有变量
        /// <summary>
        /// OPCServer Object
        /// </summary>
        private OPCServer KepServer;

        /// <summary>
        /// OPCGroups Object
        /// </summary>
        private OPCGroups KepGroups;

        /// <summary>
        /// OPCGroup Object
        /// </summary>
        private OPCGroup KepGroup;

        /// <summary>
        /// OPCItems Object
        /// </summary>
        private OPCItems KepItems;

        /// <summary>
        /// OPCItem Object
        /// </summary>
        private OPCItem KepItem;

        /// <summary>
        /// 主机IP
        /// </summary>
        private string strHostIP = "";

        /// <summary>
        /// 主机服务名称
        /// </summary>
        private string strHostSeverName = "";

        /// <summary>
        /// 连接状态
        /// </summary>
        private bool opc_connected = false;

        /// <summary>
        /// 客户端句柄
        /// </summary>
        private int itmHandleClient = 0;

        /// <summary>
        /// 服务端句柄
        /// </summary>
        private int itmHandleServer = 0;

        public Array serverHandles;
        //日志记录
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        #endregion
        #region 公有变量
        /// <summary>
        /// 获取xml文件路径
        /// </summary>
        public string SettingFilePath = "";
        public string GetDataFromDate = "2014-06-01";
        /// <summary>
        /// 获取数据的间隔秒数
        /// </summary>
        public int GetDataInterval = 30;
        /// <summary>
        /// 监控匹配分钟
        /// </summary>
        public int MatchMonitorTime = 20;
        /// <summary>
        /// 设备个数
        /// </summary>
        public int MonitorIndexNum = 10;
        public const int MonitorNum = 10;
        /// <summary>
        /// 加载设备
        /// </summary>
        public MetaMonitor[] aMO = new MetaMonitor[3];

        public List<MetaMonitor> Meta = new List<MetaMonitor>();
        /// <summary>
        /// opctag标签
        /// </summary>
        public List<string> TagitemList = new List<string>();
        /// <summary>
        /// 获取灭菌阶段时间
        /// </summary>
        public string datestring = string.Empty;

        public string year = string.Empty;
        public string month = string.Empty;
        public string date = string.Empty;
        public string hour = string.Empty;
        public string minute = string.Empty;
        public string second = string.Empty;
        public string tagname = string.Empty;
        public Dictionary<int, string> tagDiction = new Dictionary<int, string>();
        object ItemValues = new object(); object Qualities; object TimeStamps;//同步读的临时变量：值、质量、时间戳  

        public int aFDataSourceType = 0; //新华
        /// <summary>
        /// 创建线程
        /// </summary>
        Thread thread = null;

        Thread threadEvent = null;

        Array Errors;
        int cancelID;
        #endregion
        #region 枚举
        /// <summary>
        /// 程序阶段
        /// </summary>
        public enum PhaseName
        {
            脉动阶段 = 1,
            升温阶段 = 2,
            灭菌阶段 = 3,
            排气阶段 = 4,
            干燥阶段 = 5,
            结束 = 6
        }

        public enum ProgramName
        {
            织物   = 1,
            器械   = 2,
            BD     = 3,
            液体   = 4,
            自选一 = 5,
            检漏   = 6,
            干燥   = 7,
            自选二 = 8,
            预热   = 9,
            PCD    = 10
        }
        #endregion

        #region OPC操作

        /// <summary>
        /// 从配置文件中读取IP和Server名
        /// </summary>
        private void GetLocalServer()
        {
            strHostIP = ConfigurationManager.AppSettings["HostIP"].ToString();
            strHostSeverName = ConfigurationManager.AppSettings["ShinvaServerName"].ToString();
        }

        /// <summary>
        /// 连接OPC服务器
        /// </summary>
        /// <param name="remoteServerIP">OPCServerIP</param>
        /// <param name="remoteServerName">OPCServer名称</param>
        private bool ConnectRemoteServer(string remoteServerIP, string remoteServerName)
        {
            try
            {
                KepServer = new OPCServer();
                KepServer.Connect(remoteServerName, remoteServerIP);//调试

                if (KepServer.ServerState == (int)OPCServerState.OPCRunning)
                {
                    logger.Info("已连接到-" + KepServer.ServerName + "   ");
                }
                else
                {
                    //这里你可以根据返回的状态来自定义显示信息，请查看自动化接口API文档
                    logger.Debug("状态：" + KepServer.ServerState.ToString() + "   ");
                }
            }
            catch (Exception err)
            {
                logger.Debug("连接远程服务器出现错误：" + err.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 列出OPC服务器中所有节点
        /// </summary>
        /// <param name="oPCBrowser">OPC服务</param>
        private void RecurBrowse(OPCBrowser oPCBrowser)
        {
            try
            {
                //展开分支
                oPCBrowser.ShowBranches();
                //展开叶子
                oPCBrowser.ShowLeafs(true);
                foreach (MetaMonitor me in aMO)
                {
                    foreach (object turn in oPCBrowser)
                    {
                        if (turn.ToString().Contains(me.ShinvaKEPSever) && turn.ToString().LastIndexOf(".") == 9)
                        {
                            TagitemList.Add(turn.ToString());
                            logger.Info("列出OPC服务器中所有节点" + turn.ToString());
                        }
                    }
                }
                SelectedOPCTagitem(TagitemList);
            }
            catch (Exception ex)
            {
                logger.Error("列出OPC服务器中所有节点" + ex.Message + "\n" + ex.StackTrace);
                //throw;
            }
        }

        /// <summary>
        /// 创建组
        /// </summary>
        private bool CreateGroup()
        {
            try
            {
                KepGroups = KepServer.OPCGroups;
                KepGroup = KepGroups.Add("OPCDOTNETGROUP");
                SetGroupProperty();
                //KepGroup.DataChange += new DIOPCGroupEvent_DataChangeEventHandler(KepGroup_DataChange);
                //KepGroup.AsyncWriteComplete += new DIOPCGroupEvent_AsyncWriteCompleteEventHandler(KepGroup_AsyncWriteComplete);


                //以下异步读  
                //MyGroup.AsyncRead(4, ref serverHandles, out Errors, 1, out cancelID);//第一参数为item数量  

                KepItems = KepGroup.OPCItems;
                KepGroup.AsyncReadComplete += new DIOPCGroupEvent_AsyncReadCompleteEventHandler(GroupAsyncReadComplete);

            }
            catch (Exception err)
            {
                logger.Debug("创建组出现错误：" + err.Message);
                return false;
            }
            return true;
        }
        public void SETAsyncReadComplete()
        {
            KepGroup.AsyncRead(KepItems.Count, ref serverHandles, out Errors, 1, out cancelID);
        }

        /// <summary>
        /// 设置组属性
        /// </summary>
        private void SetGroupProperty()
        {
            KepServer.OPCGroups.DefaultGroupIsActive = true;
            KepServer.OPCGroups.DefaultGroupDeadband = 0;
            KepGroup.UpdateRate = 1000;
            KepGroup.IsActive = true;
            KepGroup.IsSubscribed = true;
        }

        #region 事件
        /// <summary>
        /// 每当项数据有变化时执行的事件
        /// </summary>
        /// <param name="TransactionID">处理ID</param>
        /// <param name="NumItems">项个数</param>
        /// <param name="ClientHandles">项客户端句柄</param>
        /// <param name="ItemValues">TAG值</param>
        /// <param name="Qualities">品质</param>
        /// <param name="TimeStamps">时间戳</param>
        void KepGroup_DataChange(int TransactionID, int NumItems, ref Array ClientHandles, ref Array ItemValues, ref Array Qualities, ref Array TimeStamps)
        {
            //为了测试，所以加了控制台的输出，来查看事物ID号
            //Console.WriteLine("********"+TransactionID.ToString()+"*********");
            foreach (MetaMonitor mo in aMO)
            {
                foreach (string tag in TagitemList)
                {
                    for (int i = 1; i <= NumItems; i++)
                    {
                        string value = ItemValues.GetValue(i).ToString();
                        string Qualitie = Qualities.GetValue(i).ToString();
                        string TimeStamp = TimeStamps.GetValue(i).ToString();
                        logger.Info(tag + ":" + value);
                        logger.Info(tag + ":" + Qualitie);
                        logger.Info(tag + ":" + TimeStamp);
                        //if (SetMonitorData(tag, value, Qualitie, TimeStamp, mo))
                        //{
                        //    LoadMonitorData(mo);
                        //}
                    }
                }
            }
        }
        /// <summary>
        /// 写入TAG值时执行的事件
        /// </summary>
        /// <param name="TransactionID"></param>
        /// <param name="NumItems"></param>
        /// <param name="ClientHandles"></param>
        /// <param name="Errors"></param>
        void KepGroup_AsyncWriteComplete(int TransactionID, int NumItems, ref Array ClientHandles, ref Array Errors)
        {
            //lblState.Text = "";
            //for (int i = 1; i <= NumItems; i++)
            //{
            //    lblState.Text += "Tran:" + TransactionID.ToString() + "   CH:" + ClientHandles.GetValue(i).ToString() + "   Error:" + Errors.GetValue(i).ToString();
            //}
        }
        /// <summary>  
        /// 异步读完成  
        /// 运行时，Array数组从下标1开始而非0！  
        /// </summary>  
        /// <param name="TransactionID"></param>  
        /// <param name="NumItems"></param>  
        /// <param name="ClientHandles"></param>  
        /// <param name="ItemValues"></param>  
        /// <param name="Qualities"></param>  
        /// <param name="TimeStamps"></param>  
        /// <param name="Errors"></param>  
        void GroupAsyncReadComplete(int TransactionID, int NumItems, ref System.Array ClientHandles, ref System.Array ItemValues, ref System.Array Qualities, ref System.Array TimeStamps, ref System.Array Errors)
        {


            foreach (MetaMonitor mo in aMO)
            {
                for (int i = 1; i <= NumItems; i++)
                {
                    //if(tagDiction.ContainsKey((int)ClientHandles.GetValue(i)))
                    //{
                    //      SetMonitorData(tagDiction[(int)ClientHandles.GetValue(i)].ToString().Trim(), ItemValues.GetValue(i).ToString(), mo);
                    //      logger.Info("kebitem value：" + ItemValues.GetValue(i).ToString());
                    //}
                    SetMonitorData(TagitemList[i - 1].ToString().Trim(), ItemValues.GetValue(i).ToString(), mo);
                    logger.Info("Tageitem：" + TagitemList[i - 1].ToString().Trim());
                    logger.Info("kebitem value：" + ItemValues.GetValue(i).ToString());

                }
                datestring = year + "-" + month + "-" + date + " " + hour + ":" + minute + ":" + second;
                ////mo.FDate = Convert.ToDateTime(datestring);
                if (!string.IsNullOrEmpty(mo.ProgramName))
                {

                    LoadMonitorData(mo);
                }

                KepItems.Remove(KepItems.Count, ref serverHandles, out Errors);
            }

        }



        #endregion

        /// <summary>
        /// 断开OPC服务器
        /// </summary>
        /// <param name="remoteServerIP">OPCServerIP</param>
        /// <param name="remoteServerName">OPCServer名称</param>
        private void DisconnectRemoteServer(string remoteServerIP, string remoteServerName)
        {
            if (!opc_connected)
            {
                return;
            }

            if (KepGroup != null)
            {
                KepGroup.DataChange -= new DIOPCGroupEvent_DataChangeEventHandler(KepGroup_DataChange);
                KepGroup.AsyncReadComplete -= new DIOPCGroupEvent_AsyncReadCompleteEventHandler(GroupAsyncReadComplete);
            }

            if (KepServer != null)
            {
                KepServer.Disconnect();
                KepServer = null;
            }

            opc_connected = false;
        }

        /// <summary>
        /// 遍历OPC服务的Tagitem
        /// </summary>
        /// <param name="taglist"></param>
        private void SelectedOPCTagitem(List<string> taglist)
        {
            try
            {
                logger.Info("taglist：" + taglist.Count.ToString());
                foreach (MetaMonitor mo in aMO)
                {
                    List<string> tagNameList = new List<string>();
                    foreach (string tag in taglist)
                    {
                        if (tag.Contains(mo.ShinvaKEPSever))
                        {
                            tagNameList.Add(tag);
                        }
                    }
                    foreach (string tagitem in tagNameList)
                    {

                        #region MyRegion
                        if (itmHandleClient != 0)
                        {
                            Array Errors;
                            OPCItem bItem = KepItems.GetOPCItem(itmHandleServer);
                            //注：OPC中以1为数组的基数
                            int[] temp = new int[2] { 0, bItem.ServerHandle };
                            Array serverHandle = (Array)temp;
                            //移除上一次选择的项
                            KepItems.Remove(KepItems.Count, ref serverHandle, out Errors);
                        }
                        //Thread.Sleep(1000);
                        itmHandleClient = 1234;
                        logger.Info("遍历OPC服务的Tagitem：" + tagitem.ToString());
                        try
                        {
                            KepItem = KepItems.AddItem(tagitem.ToString().Trim(), itmHandleClient);
                            itmHandleServer = KepItem.ServerHandle;
                            tagname = tagitem;
                            Thread.Sleep(1005);
                            #region 同步读
                            KepItem.Read(1, out ItemValues, out Qualities, out TimeStamps);
                            logger.Info("tagName：" + tagitem.ToString());
                            logger.Info("kebitem value：" + ItemValues.ToString());
                            #endregion
                            #region 异步读取
                            //    int[] temp = new int[2] { 0, itmHandleServer };
                            //    Array serverHandles = (Array)temp;
                            //    Array Errors;
                            //    int cancelID;
                            //    KepGroup.AsyncRead(KepItems.Count, ref serverHandles, out Errors, 1, out cancelID);
                            //    KepGroup.AsyncReadComplete += new DIOPCGroupEvent_AsyncReadCompleteEventHandler(GroupAsyncReadComplete);
                            //    // KepServer.OPCGroups.RemoveAll();
                            #endregion
                            SetMonitorData(tagitem.ToString().Trim(), ItemValues.ToString(), mo);
                        }
                        catch (Exception ex)
                        {
                            if (itmHandleClient != 0)
                            {
                                Array Errors;
                                OPCItem bItem = KepItems.GetOPCItem(itmHandleServer);
                                //注：OPC中以1为数组的基数
                                int[] temp = new int[2] { 0, bItem.ServerHandle };
                                Array serverHandle = (Array)temp;
                                //移除上一次选择的项
                                KepItems.Remove(KepItems.Count, ref serverHandle, out Errors);
                            }
                            itmHandleClient = 0;
                            logger.Error("此项为系统保留项:" + ex.Message + "\n" + ex.StackTrace);
                            //ConnectRemoteServerNO(strHostIP, strHostSeverName);
                            //throw;
                        }

                        #endregion
                    }
                    if (!string.IsNullOrEmpty(mo.ProgramName))
                    {
                        datestring = DateTime.Now.Year.ToString() + "-" + mo.month + "-" + mo.date + " " + mo.hour + ":" + mo.minute + ":" + mo.second;

                        mo.FDate = Convert.ToDateTime(datestring);
                        logger.Info("日期：" + datestring);

                        LoadMonitorData(mo);
                    }
                    //datestring = DateTime.Now.Year.ToString() + "-" + month + "-" + date + " " + hour + ":" + minute + ":" + second;

                    //mo.FDate = Convert.ToDateTime(datestring);
                    //logger.Info("日期：" + datestring);
                    //if (!string.IsNullOrEmpty(mo.ProgramName))
                    //{
                    //    mo.FStatus = "Run";
                    //    LoadMonitorData(mo);
                    //}
                    #region 异步读取
                    //int[] temp = new int[tempi.Count];
                    //for (int i = 0; i < tempi.Count; i++)
                    //{
                    //    temp[i] = tempi[i];
                    //}
                    //serverHandles = (Array)temp;

                    ////KepGroup.AsyncRead(KepItems.Count, ref serverHandles, out Errors, 1, out cancelID);

                    ////Thread.Sleep(30000);
                    //threadEvent = new Thread(SETAsyncReadComplete);
                    //threadEvent.Start();
                    ////KepGroup.AsyncReadComplete += new DIOPCGroupEvent_AsyncReadCompleteEventHandler(GroupAsyncReadComplete);


                    //KepItems.Remove(KepItems.Count, ref serverHandles, out Errors);
                    //KepGroup.AsyncReadComplete -= new DIOPCGroupEvent_AsyncReadCompleteEventHandler(GroupAsyncReadComplete);
                    #endregion



                    //Thread.Sleep(1000);
                }

            }
            catch (Exception err)
            {
                //没有任何权限的项，都是OPC服务器保留的系统项，此处可不做处理。
                //itmHandleClient = 0;
                logger.Error("此项为系统保留项:" + err.Message + "\n" + err.StackTrace);
                //ConnectRemoteServerNO(strHostIP, strHostSeverName);
                //throw;
            }
        }


        private bool SetMonitorData(string tag, string value, MetaMonitor mo)// string Qualities, string TimeStamp, MetaMonitor mo)
        {
            string item = tag.Substring(tag.LastIndexOf(".") + 1, tag.Length - tag.LastIndexOf(".") - 1);
            string Devicename = tag.Substring(0, tag.LastIndexOf("."));//tag.LastIndexOf(".") - 1);
            logger.Info("获取数据：" + item);
            logger.Info("获取数据：" + Devicename);
            if (Devicename == mo.ShinvaKEPSever)
            {
                switch (item)
                {
                    case "R001"://腔内压力
                        switch (mo.DeviceType)
                        {
                            case 4:
                                mo.FValue2 = Convert.ToDouble(Convert.ToInt32(value) - 100);
                                logger.Info("腔内压力：" + mo.FValue2.ToString());
                                break;
                            case 1:
                                break;
                        }
                        break;
                    case "R002"://腔内温度
                        switch (mo.DeviceType)
                        {
                            case 4:
                                mo.FValue1 = Convert.ToDouble(Convert.ToInt32(value) / 10);
                                logger.Info("腔内温度：" + mo.FValue1.ToString());
                                break;
                            case 1:
                                mo.FValue1 = Convert.ToDouble(Convert.ToInt32(value) / 10);
                                break;
                        }
                        break;
                    case "R003"://夹层压力
                        switch (mo.DeviceType)
                        {
                            case 4:
                                mo.FValue3 = Convert.ToDouble(Convert.ToInt32(value) - 100);
                                logger.Info("夹层压力：" + mo.FValue3.ToString());
                                break;
                            case 1:
                                break;
                        }
                        break;
                    case "R004"://夹层温度
                        switch (mo.DeviceType)
                        {
                            case 4:
                                mo.FValue4 = Convert.ToDouble(Convert.ToInt32(value) / 10);
                                logger.Info("夹层温度：" + mo.FValue4.ToString());
                                break;
                            case 1:
                                mo.FValue4 = Convert.ToDouble(Convert.ToInt32(value) / 10);
                                break;
                        }
                        break;
                    case "R006"://记录温度
                        //switch (mo.DeviceType)
                        //{
                        //    case 4:
                        //        mo.FValue2 = Convert.ToDouble(value);
                        //        break;
                        //    case 1:
                        //        mo.FValue2 = Convert.ToDouble(value);
                        //        break;
                        //}
                        break;
                    case "R0052"://操作员号
                        //switch (mo.DeviceType)
                        //{
                        //    case 4:
                        //        mo.FValue3 = Convert.ToDouble(value);
                        //        break;
                        //    case 1:
                        //        mo.FValue3 = Convert.ToDouble(value);
                        //        break;
                        //}
                        break;
                    case "R065"://年
                        switch (mo.DeviceType)
                        {
                            case 4:
                                year = value;
                                logger.Info("年：" + year.ToString());
                                break;
                            case 1:
                                year = value;
                                break;
                        }
                        break;
                    case "R066"://月
                        switch (mo.DeviceType)
                        {
                            case 4:
                                //datestring = datestring + "-" + value;
                                mo.month = value;
                                logger.Info("月：" + mo.month.ToString());
                                break;
                            case 1:
                                //datestring = datestring + "-" + value;
                                month = value;
                                break;
                        }
                        break;
                    case "R067"://日
                        switch (mo.DeviceType)
                        {
                            case 4:
                                //datestring = datestring + "-" + value;
                                mo.date = value;
                                logger.Info("日：" + mo.date.ToString());
                                break;
                            case 1:
                                //datestring = datestring + "-" + value;
                                date = value;
                                break;
                        }
                        break;
                    case "R068"://时
                        switch (mo.DeviceType)
                        {
                            case 4:
                                //datestring = datestring + " " + value;
                                mo.hour = value;
                                logger.Info("时：" + mo.hour.ToString());
                                break;
                            case 1:
                                //datestring = datestring + " " + value;
                                hour = value;
                                break;
                        }
                        break;
                    case "R069"://分
                        switch (mo.DeviceType)
                        {
                            case 4:
                                //datestring = datestring + ":" + value;
                                mo.minute = value;
                                logger.Info("分：" + mo.minute.ToString());
                                break;
                            case 1:
                                //datestring = datestring + ":" + value;
                                minute = value;
                                break;
                        }
                        break;
                    case "R070"://秒
                        switch (mo.DeviceType)
                        {
                            case 4:
                                //datestring = datestring + ":" + value;
                                mo.second = value;
                                logger.Info("秒：" + mo.second.ToString());
                                break;
                            case 1:
                                //datestring = datestring + ":" + value;
                                second = value;

                                break;
                        }
                        break;
                    case "R077"://运转次
                        switch (mo.DeviceType)
                        {
                            case 4:
                                mo.FDataIndex = Convert.ToDecimal(value);
                                logger.Info("运转次数：" + mo.FDataIndex.ToString());
                                break;
                            case 1:
                                mo.FDataIndex = Convert.ToDecimal(value);
                                break;
                        }
                        break;
                    case "R087"://灭菌时间
                        switch (mo.DeviceType)
                        {
                            case 4:
                                mo.FValue5 = Convert.ToDouble(Convert.ToInt32(value) / 60);
                                logger.Info("灭菌时间：" + mo.FValue5.ToString());
                                break;
                            case 1:
                                mo.FValue5 = Convert.ToDouble(Convert.ToInt32(value) / 60);
                                break;
                        }
                        break;
                    case "R089"://灭菌温度
                        switch (mo.DeviceType)
                        {
                            case 4:
                                mo.FValue6 = Convert.ToDouble(Convert.ToInt32(value) / 10);
                                logger.Info("灭菌温度：" + mo.FValue6.ToString());
                                break;
                            case 1:
                                mo.FValue6 = Convert.ToDouble(Convert.ToInt32(value) / 10);
                                break;
                        }
                        break;
                    case "R095"://程序类别
                        switch (mo.DeviceType)
                        {
                            case 4:
                                switch (value)
                                {
                                    case "1":
                                        mo.ProgramName = ProgramName.织物.ToString();
                                        mo.FStatus = "Run";
                                        break;
                                    case "2":
                                        mo.ProgramName = ProgramName.器械.ToString();
                                        mo.FStatus = "Run";
                                        break;
                                    case "3":
                                        mo.ProgramName = ProgramName.BD.ToString();
                                        mo.FStatus = "Run";
                                        break;
                                    case "4":
                                        mo.ProgramName = ProgramName.液体.ToString();
                                        mo.FStatus = "Run";
                                        break;
                                    case "5":
                                        mo.ProgramName = ProgramName.自选一.ToString();
                                        mo.FStatus = "Run";
                                        break;
                                    case "6":
                                        mo.ProgramName = ProgramName.检漏.ToString();
                                        mo.FStatus = "Run";
                                        break;
                                    case "7":
                                        mo.ProgramName = ProgramName.干燥.ToString();
                                        mo.FStatus = "Run";
                                        break;
                                    case "8":
                                        mo.ProgramName = ProgramName.自选二.ToString();
                                        mo.FStatus = "Run";
                                        break;
                                    case "9":
                                        mo.ProgramName = ProgramName.预热.ToString();
                                        mo.FStatus = "Run";
                                        break;
                                    case "10":
                                        mo.ProgramName = ProgramName.PCD.ToString();
                                        mo.FStatus = "Run";
                                        break;
                                    default:
                                        break;

                                }
                                logger.Info("程序类别：" + mo.ProgramName.ToString());
                                break;
                            case 1:
                                break;
                        }
                        break;
                    case "R097"://程序阶段
                        switch (mo.DeviceType)
                        {
                            case 4:
                                switch (value)
                                {
                                    case "1":
                                        mo.PhaseName = PhaseName.脉动阶段.ToString();
                                        break;
                                    case "2":
                                        mo.PhaseName = PhaseName.升温阶段.ToString();
                                        break;
                                    case "3":
                                        mo.PhaseName = PhaseName.灭菌阶段.ToString();
                                        break;
                                    case "4":
                                        mo.PhaseName = PhaseName.排气阶段.ToString();
                                        break;
                                    case "5":
                                        mo.PhaseName = PhaseName.干燥阶段.ToString();
                                        break;
                                    case "6":
                                        mo.PhaseName = PhaseName.结束.ToString();
                                        mo.FStatus = "End";
                                        break;
                                }

                                break;
                            case 1:
                                break;
                        }
                        logger.Info("程序阶段：" + mo.PhaseName.ToString());
                        break;
                    default:
                        break;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        public void Connect()//Main
        {
            while (true)
            {
                if (ConnectRemoteServer(strHostIP, strHostSeverName))
                {
                    opc_connected = true;
                    if (CreateGroup())
                    {
                        break;
                    }
                    if (KepServer != null)
                    {
                        KepServer.Disconnect();
                        KepServer = null;
                    }
                    opc_connected = false;
                }
                Thread.Sleep(3000);
            }
            while (true)
            {
                TagitemList.Clear();
                RecurBrowse(KepServer.CreateBrowser());
            }
        }

        protected void OnStart()
        {
            thread = new Thread(Connect);
            thread.Start();
        }

        protected void OnStop()
        {
            DisconnectRemoteServer(strHostIP, strHostSeverName);
            thread.Abort();
        }

        /// <summary>
        /// 获取xml文件路径
        /// </summary>
        /// <returns></returns>
        private string GetExePath()
        {
            try
            {
                string assemblyFilePath = Assembly.GetExecutingAssembly().Location;
                string assemblyDirPath = Path.GetDirectoryName(assemblyFilePath);
                return assemblyDirPath;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        ///  加载设备参数
        /// </summary>
        public void InitVar()
        {
            try
            {

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(SettingFilePath);
                //取数间隔秒数
                XmlNode xn = xmlDoc.SelectSingleNode("config/GetDataInterval");
                GetDataInterval = xn == null ? 120000 : Convert.ToInt16(xn.Attributes[0].Value) * 1000;
                try
                {
                    //获取取Shinva监控数据最开始日期
                    XmlNode xn2 = xmlDoc.SelectSingleNode("config/GetDataFromDate");
                    GetDataFromDate = xn2 == null ? "2014-05-01" : xn2.Attributes[0].Value.ToString();

                    GetDataFromDate = Convert.ToDateTime(GetDataFromDate).ToString("yyyy-MM-dd HH:mm:ss");
                }
                catch { GetDataFromDate = "2014-05-01 00:00:00"; }
                //监控数据匹配时间
                XmlNode xn10 = xmlDoc.SelectSingleNode("config/MatchMonitorTime");
                MatchMonitorTime = Convert.ToInt32(xn10.Attributes[0].Value.Trim());
            }
            catch (Exception ex)
            {
                logger.Debug("Infecon.Monitor.Service.Shinva" + "CSSD监控初始化设置信息出错," + ex.Message);
            }
        }

        /// <summary>
        /// 加载设备信息
        /// </summary>
        public void InitMonitorDevice()
        {
            try
            {
                logger.Info("InitMonitorDevice");

                System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
                xmlDoc.Load(SettingFilePath);

                XmlNodeList nodes = xmlDoc.SelectNodes("config/DeviceConfig/MonitorDevice");

                MonitorIndexNum = nodes.Count;

                for (int j = 0; j < nodes.Count; j++)
                {
                    aMO[j] = new MetaMonitor();
                    aMO[j].index = j;
                    aMO[j].Barcode = "0";

                    aMO[j].FLogID = "";
                    aMO[j].FID = 0;
                    aMO[j].PFID = 0;
                    aMO[j].ProgramNo = 0;
                    aMO[j].ProgramName = "";
                    aMO[j].Phase = 0;
                    aMO[j].PhaseName = "";
                    aMO[j].FValue1 = 0;
                    aMO[j].FValue2 = 0;
                    aMO[j].FValue3 = 0;
                    aMO[j].FValue4 = 0;
                    aMO[j].FValue5 = 0;
                    aMO[j].FValue6 = 0;
                }

                int i = 0;

                foreach (XmlNode xn in nodes)
                {
                    aMO[i].DeviceType = Convert.ToInt32(xn.Attributes[0].Value.ToString().Trim());
                    aMO[i].DeviceName = xn.Attributes[1].Value.ToString().Trim();
                    aMO[i].Barcode = xn.Attributes[2].Value.ToString().Trim();
                    aMO[i].ShinvaKEPSever = xn.Attributes[3].Value.ToString().Trim();
                    aMO[i].DeviceID = GetDeviceIDByBarcode(aMO[i].Barcode).ToUpper();
                    aMO[i].FLogID = "";
                    aMO[i].FID = 0;
                    aMO[i].PFID = 0;
                    aMO[i].ProgramNo = 0;
                    aMO[i].ProgramName = "";
                    aMO[i].Phase = 0;
                    aMO[i].PhaseName = "";
                    aMO[i].FDataIndex = 0;
                    aMO[i].FLogID = "";
                    aMO[i].DeviceValidMsg = VerifyInfeconDevice(aMO[i].DeviceType, aMO[i].Barcode.ToString());
                    logger.Info(aMO[i].ShinvaKEPSever);
                    i = i + 1;
                }
            }
            catch (Exception ex)
            {
                logger.Debug("Infecon.Monitor.Service.Shinva" + "CSSD监控设备初始化加载出错," + ex.Message);
            }
        }

        /// <summary>
        /// 更新监控队列和更新监控集合
        /// </summary>
        /// <param name="dv"></param>
        public void UpdateDeviceQueue(MetaMonitor aCurrent)
        {
            string InsertSQL = string.Format(@"insert into MonitorDeviceQueue( FLogID,
                                                                                      FDataIndex,
                                                                                      FDeviceID,
                                                                                      FBeginDate,
                                                                                      FEndDate,
                                                                                      FStatus)
                                                     values('{0}',{1},'{2}','{3}','{4}','{5}')",
                                             aCurrent.FLogID.ToUpper(),
                                             aCurrent.FDataIndex,
                                             aCurrent.Barcode.ToString(),
                                             aCurrent.BeginDate.ToString("yyyy-MM-dd HH:mm:ss"),
                                             aCurrent.EndDate.ToString("yyyy-MM-dd HH:mm:ss"),
                                             0);


            string InsertSQL2 = string.Format(@"insert into MonitorCollectData(  
                                                                                          FID,
                                                                                          FLogID,
                                                                                          FDeviceID,
                                                                                          FMonitorType,
                                                                                          FDataSourceType,

                                                                                          FBeginDate,
                                                                                          FEndDate,
                                                                                          FSystemNo,
                                                                                          FVersion,
                                                                                          FProgramNo,

                                                                                          FPargramName,
                                                                                          FProgramPhrase,
                                                                                          FBatchNo,
                                                                                          FHasCompress,
                                                                                          FZoneNo,

                                                                                          FDataIndex)
                                                                select FID,'{0}','{1}',{2},{3},'{4}','{5}',{6},{7},{8},'{9}','{10}',{11},{12},{13},{14}
                                                                from MonitorDeviceQueue where FDeviceID='{1}' and FDataIndex={14} order by FID desc",
                                                                    "00000000-0000-0000-0000-000000000000",
                                                                    aCurrent.Barcode.ToString(),
                                                                    aCurrent.DeviceType,
                                                                    aFDataSourceType,
                                                                    aCurrent.BeginDate.ToString("yyyy-MM-dd HH:mm:ss"),
                                                                    aCurrent.EndDate.ToString("yyyy-MM-dd HH:mm:ss"),
                                                                    0,
                                                                    0,
                                                                    aCurrent.ProgramNo,
                                                                    aCurrent.ProgramName,
                                                                    aCurrent.PhaseName,
                                                                    aCurrent.BatchNO,
                                                                    0,
                                                                    0,
                                                                    aCurrent.FDataIndex);


            try
            {
                int i = DBProvider.ExecuteNonQuery(InsertSQL);

                int a = DBProvider.ExecuteNonQuery(InsertSQL2);

            }
            catch (Exception ex)
            {
                logger.Error("UpdateDeviceQueue: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        #region 初始化设备信息
        public void LoadMonitorData(MetaMonitor dv)
        {
            try
            {
                dv.BatchNO = Convert.ToInt32(dv.FDataIndex.ToString());
                dv.BeginDate = DateTime.Now;// Convert.ToDateTime(dv.FDate);
                dv.EndDate = dv.BeginDate;
                dv.PFID = GetMonitorID(dv);
                if (dv.PFID < 1)
                {
                    dv.FLogID = Guid.NewGuid().ToString();
                    UpdateDeviceQueue(dv);
                    dv.PFID = GetMonitorIDByFDataIndex(dv);
                }
                if (dv.PFID > 0)
                {
                    //Monitor匹配记录
                    UpdateMatchCSSD(dv);

                    //更新MonitorCollectDataEntry明细信息
                    UpdateCollectDataEntry(dv);

                    //更新状态
                    UpdateMonitorStatus(dv);
                }
            }
            catch (Exception ex)
            {
                logger.Error("LoadMonitorData: " + ex.Message + "\n" + ex.StackTrace);
            }
        }
        #endregion
        /// <summary>
        /// 获取FID
        /// </summary>
        /// <param name="aCurrent"></param>
        /// <returns></returns>
        public int GetMonitorID(MetaMonitor aCurrent)
        {
            int PFID = 0;
            try
            {
                Infecon.CSSD.Business.BusinessBase bll = new Infecon.CSSD.Business.BusinessBase();
                MonitorCollectDataEntity mon = (MonitorCollectDataEntity)bll.SelectSingle(typeof(MonitorCollectDataEntity),
                                                                      string.Format("FDEVICEID='{0}' and  FDataIndex={1} ",
                                                                                         aCurrent.Barcode.ToString(),
                                                                                         aCurrent.FDataIndex), "FID Desc");

                if (mon == null)
                    PFID = 0;
                else
                    PFID = mon.FID;
            }
            catch { }

            return PFID;

        }

        public int GetMonitorIDByFDataIndex(MetaMonitor aCurrent)
        {
            int PFID = 0;
            try
            {
                Infecon.CSSD.Business.BusinessBase bll = new Infecon.CSSD.Business.BusinessBase();
                MonitorDeviceQueueEntity mon = (MonitorDeviceQueueEntity)bll.SelectSingle(typeof(MonitorDeviceQueueEntity),
                                                                      string.Format("FDEVICEID='{0}' and  FDataIndex={1} ",
                                                                                         aCurrent.Barcode.ToString(),
                                                                                         aCurrent.FDataIndex), "FID Desc");

                if (mon == null)
                    PFID = 0;
                else
                    PFID = mon.FID;
            }
            catch { }

            return PFID;

        }

        public void UpdateMatchCSSD(MetaMonitor dv)
        {
            try
            {
                string aCssdLogID = "";
                DateTime aMatchBeginDate = dv.BeginDate.AddMinutes(-1 * MatchMonitorTime);
                DateTime aMatchEndDate = dv.BeginDate.AddMinutes(MatchMonitorTime);

                List<Infecon.Common.Utility.QueryParameter> ltParams = new List<Infecon.Common.Utility.QueryParameter>();

                ltParams.Add(new Infecon.Common.Utility.QueryParameter("DEVICEID", Infecon.Common.Utility.OperateSymbol.EqualTo, dv.DeviceID, "System.String"));
                ltParams.Add(new QueryParameter("BEGINDATE", OperateSymbol.NotLessThan, aMatchBeginDate, "DateTime"));
                ltParams.Add(new QueryParameter("BEGINDATE", OperateSymbol.NotGreaterThan, aMatchEndDate, "DateTime"));

                string sql = @"select  LOGID  from DEVICESUSELOG  where 1=1 {0}";

                DataTable dt = DBProvider.ExecuteDataTable(sql, ltParams);

                int aCount = dt.Rows.Count;

                if (aCount > 0 && dt != null)
                {
                    aCssdLogID = dt.Rows[0][0].ToString().ToUpper();

                    if (aCssdLogID == "00000000-0000-0000-0000-000000000000") return;

                    using (TransactionScope tran = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 5, 0)))
                    {
                        string aSQL = string.Format("update DEVICESUSELOG set MONITORDATAID={1}  WHERE LOGID='{0}'", aCssdLogID, (int)dv.PFID);
                        string aSQL2 = string.Format("update MONITORCOLLECTDATA set FLOGID='{1}'  WHERE FID='{0}'", (int)dv.PFID, aCssdLogID);

                        DBProvider.ExecuteNonQuery(aSQL);
                        DBProvider.ExecuteNonQuery(aSQL2);

                        tran.Complete();
                    }
                }
            }
            catch { }
        }

        public void UpdateCollectDataEntry(MetaMonitor aCurrent)
        {
            string InsertSQL3 = string.Format(@"insert into MonitorCollectDataEntry( 
                                                                                  FParentID,
                                                                                  FDate,
                                                                                  FValue1,
                                                                                  FValue2,
                                                                                  FValue3,
                                                                                  FValue4,
                                                                                  FValue5,
                                                                                  FValue6,
                                                                                  FCurrentStatus,
                                                                                  FProgramPhrase,
                                                                                  FDataIndex 
                                                                                 )
                                        select  FID,'{1}',{2},{3},{4},{5},{6},{7},'{8}','{9}',{10}
                                        from  MonitorCollectData 
                                        where  FDataIndex={10}  and FDeviceID='{11}'",
                                  aCurrent.PFID,
                                  aCurrent.FDate.ToString("yyyy-MM-dd HH:mm:ss"),
                                  aCurrent.FValue1,
                                  aCurrent.FValue2,
                                  aCurrent.FValue3,
                                  aCurrent.FValue4,
                                  aCurrent.FValue5,
                                  aCurrent.FValue6,
                                  aCurrent.FStatus,
                                  aCurrent.PhaseName,
                                  aCurrent.FDataIndex,
                                  aCurrent.Barcode
                                 );


            try
            {
                DBProvider.ExecuteNonQuery(InsertSQL3);

                if (aCurrent.PhaseName.Equals(PhaseName.结束.ToString()))  //结束 
                {
                    try
                    {
                        MonitorCollectDataEntity aCollectDataEntry = (MonitorCollectDataEntity)DBProvider.SelectSingle(typeof(MonitorCollectDataEntity),
                                                                                    string.Format("FID={0} And FDEVICEID='{1}'",
                                                                                    aCurrent.PFID, aCurrent.Barcode), "FID DESC");
                        aCollectDataEntry.FEndDate = aCurrent.FDate;

                        DBProvider.Update(aCollectDataEntry);
                    }
                    catch { }

                }
            }
            catch { }

            UpdateMatchCSSD(aCurrent);
        }

        private void UpdateMonitorStatus(MetaMonitor aCurrent)
        {
            try
            {
                string SQL_UP_DEVICESTATUS = string.Format(@"UPDATE DevicesStatus SET FCurrentStatus='{1}' ,FRecordID={2} WHERE FDeviceID='{0}'",
                                                         aCurrent.Barcode, aCurrent.FStatus, aCurrent.PFID);

                string SQL_UP_COLLECTDATA_ENDDATE = string.Format(@"update MonitorCollectData set FEndDate='{2}'  where  FDeviceID='{0}' and FID={1}",
                                                               aCurrent.Barcode, aCurrent.PFID, aCurrent.FDate.ToString("yyyy-MM-dd HH:mm:ss"));

                string SQL_UP_DEVICEQUEUE_ENDDATE = string.Format(@"update MonitorDeviceQueue set FEndDate='{2}'  where  FDeviceID='{0}' and FID={1}",
                                                              aCurrent.Barcode, aCurrent.PFID, aCurrent.FDate.ToString("yyyy-MM-dd HH:mm:ss"));

                int i = DBProvider.ExecuteNonQuery(SQL_UP_DEVICESTATUS);

                DBProvider.ExecuteNonQuery(SQL_UP_COLLECTDATA_ENDDATE);

                DBProvider.ExecuteNonQuery(SQL_UP_DEVICEQUEUE_ENDDATE);
            }
            catch { }
        }


        /// <summary>
        ///   验证设备是否存在
        /// </summary>
        public string GetDeviceIDByBarcode(string aBarcode)
        {
            try
            {
                Infecon.CSSD.Business.BusinessBase bll = new Infecon.CSSD.Business.BusinessBase();
                DevicesEntity dv = (DevicesEntity)bll.SelectSingle(typeof(DevicesEntity), string.Format("BARCODE='{0}'", aBarcode), string.Empty);
                return dv.DeviceID.ToString().ToUpper();
            }
            catch { return ""; }

        }

        /// <summary>
        /// 验证设备是否存在
        /// </summary>
        public string VerifyInfeconDevice(int aDeviceType, string aBarcode)
        {
            try
            {
                Infecon.CSSD.Business.BusinessBase bll = new Infecon.CSSD.Business.BusinessBase();
                DevicesEntity dv = (DevicesEntity)bll.SelectSingle(typeof(DevicesEntity), string.Format("BARCODE='{0}' and  DEVICETYPE={1}", aBarcode, aDeviceType), string.Empty);

                if (dv == null)
                    return ";请检查,该Infecon设备类型与设备ID信息不匹配。";
                else
                    return "";
            }
            catch { return ";请检查,该Infecon设备类型与设备ID信息不匹配。"; }

        }

        private void button1_Click(object sender, EventArgs e)//start
        {
            OnStart();
        }

        private void button2_Click(object sender, EventArgs e)//stop
        {
            OnStop();
        }
    }
}
