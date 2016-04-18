using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Configuration;
using System.Xml;
using System.Reflection;
using System.IO;
using System.IO.Ports;
using Infecon.Common.Utility;
using System.Data.OleDb;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Transactions;

using log4net;
using Infecon.CSSD.Entity;
using Infecon.CSSD.Business;
using Infecon.CSSD.Entity.Monitor;

using System.Diagnostics;
using System.Data.OracleClient;

namespace WindowsService3
{
    public partial class Service1 : ServiceBase
    {
        System.Timers.Timer timerGlobalSettings;
        public bool NextTimeFlag = true;
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public Service1()
        {
            InitializeComponent();
            timerGlobalSettings = new System.Timers.Timer(1000);
        }

        protected override void OnStart(string[] args)
        {
            timerGlobalSettings.Elapsed += new System.Timers.ElapsedEventHandler(timerGlobalSetting_Tick);
            timerGlobalSettings.AutoReset = true;
            timerGlobalSettings.Enabled = true;
            logger.Info("Start: " + DateTime.Now.ToShortDateString());
        }
        public void timerGlobalSetting_Tick(object source, System.Timers.ElapsedEventArgs e)
        {
            //while (!NextTimeFlag)
            //{
                //Thread.Sleep(1000);
            //}

            NextTimeFlag = false;

            logger.Info("info: " + DateTime.Now.ToShortDateString());

            NextTimeFlag = true;

        }
        protected override void OnStop()
        {
            logger.Info("stop: " + DateTime.Now.ToShortDateString());
        }
    }
}
