using System;
using System.Collections;
using log4net;
using Quartz;
using Quartz.Spi;
using System.Net.Sockets;
using System.Net;

namespace Infecon.CSSD.Monitor.SmartNode
{
    /// <summary>
    /// 插件：读取Belimed传感器数据
    /// </summary>
    public class SmartNodePlugin : ISchedulerPlugin
    {
        // 日志记录
        protected readonly ILog logger;

        private const string cPluginName = "SmartNodePlugin";

        private const string cJobIdDataRead = "JobId_DataRead";
        private const string cTriggerIdDataRead = "TriggerId_DataRead";

        private const string cJobIdDataAnalyse = "JobId_DataAnalyse";
        private const string cTriggerIdDataAnalyse = "TriggerId_DataAnalyse";

        // 插件名称
        private string mName;
        // 调度器
        private IScheduler mSched;

        private TcpListener mServer;

        /// <summary>
        /// 参数：设备组
        /// </summary>
        public string SensorGroup
        {
            set;
            get;
        }

        /// <summary>
        /// 参数：侦听的IP
        /// </summary>
        public string TcpListenerIP
        {
            set;
            get;
        }

        /// <summary>
        /// 参数：侦听的端口号
        /// </summary>
        public int? TcpListenerPort
        {
            set;
            get;
        }

        /// <summary>
        /// 参数：读取的时间间隔（单位：毫秒）
        /// </summary>
        [TimeSpanParseRule(TimeSpanParseRule.Milliseconds)]
        public TimeSpan ReadingInterval
        {
            get;
            set;
        }

        /// <summary>
        /// 参数：解析接收数据的时间间隔（单位：秒）
        /// </summary>
        [TimeSpanParseRule(TimeSpanParseRule.Seconds)]
        public TimeSpan AnalyseInterval
        {
            get;
            set;
        }

        public SmartNodePlugin()
        {
            logger = LogManager.GetLogger(GetType());
        }

        #region ISchedulerPlugin 成员

        public void Initialize(string pluginName, Quartz.IScheduler sched)
        {
            logger.Info("SmartNodePlugin 开始初始化");
            mName = pluginName;
            mSched = sched;
            logger.Info("SmartNodePlugin 初始化结束");
        }

        public void Shutdown()
        {
            logger.Info("关闭侦听器……");
            if (mServer != null)
            {
                mServer.Stop();
                mServer = null;
            }
            logger.Info("成功关闭侦听器。");
        }

        public void Start()
        {
            logger.Info("启动传感器监控插件……");

            //System.Threading.Thread.Sleep(10 * 1000);

            // 参数检验
            if (CheckParams())
            {
                throw new Exception("插件的参数设定不正确。");
            }


            // 打开侦听器
            try
            {
                IPAddress localAddr = IPAddress.Parse(TcpListenerIP);
                mServer = new TcpListener(localAddr, TcpListenerPort.Value);
                //mServer = new TcpListener(IPAddress.Any, TcpListenerPort.Value);
                mServer.Start();
                logger.Info("成功打开侦听器。");
            }
            catch (Exception ex)
            {
                logger.Error("打开侦听器失败！", ex);
                throw ex;
            }

            ArrayList readedBuf = new ArrayList();

            IJobDetail jobRead = JobBuilder.Create<DataReadJob>()
                    .WithIdentity(String.Format("{0}_{1}", mName, cJobIdDataRead), cPluginName)
                    .Build();
            jobRead.JobDataMap.Put(Consts.C_TCP_LISTENER_KEY, mServer);
            jobRead.JobDataMap.Put(Consts.C_READED_BUFFER_KEY, readedBuf);

            ITrigger triggerRead = TriggerBuilder.Create()
                .WithIdentity(String.Format("{0}_{1}", mName, cTriggerIdDataRead), cPluginName)
                .StartNow()
                .WithSimpleSchedule(x => x.WithInterval(ReadingInterval).RepeatForever())
                .Build();

            IJobDetail jobAnalyse = JobBuilder.Create<DataAnalyseJob>()
                    .WithIdentity(String.Format("{0}_{1}", mName, cJobIdDataAnalyse), cPluginName)
                    .Build();
            jobAnalyse.JobDataMap.Put(Consts.C_READED_BUFFER_KEY, readedBuf);
            jobAnalyse.JobDataMap.Put(Consts.C_SENSOR_GROUP_KEY, SensorGroup);

            ITrigger triggerAnalyse = TriggerBuilder.Create()
                .WithIdentity(String.Format("{0}_{1}", mName, cTriggerIdDataAnalyse), cPluginName)
                .StartNow()
                .WithSimpleSchedule(x => x.WithInterval(AnalyseInterval).RepeatForever())
                .Build();

            try
            {
                // 安排运行任务
                mSched.ScheduleJob(jobRead, triggerRead);
                mSched.ScheduleJob(jobAnalyse, triggerAnalyse);
                //mSched.ScheduleJob(jobMatch, triggerMatch);
            }
            catch (SchedulerException e)
            {
                logger.Error("SmartNodePlugin 无法调度任务！");
                throw e;
            }

            logger.Info("正常启动传感器监控插件。");
        }

        private bool CheckParams()
        {
            bool isError = false;
            if (string.IsNullOrEmpty(SensorGroup))
            {
                logger.Error("请设定参数：sensorGroup的值。");
                isError = true;
            }
            if (string.IsNullOrEmpty(TcpListenerIP))
            {
                logger.Error("请设定参数：tcpListenerIP的值。");
                isError = true;
            }
            if (TcpListenerPort == null)
            {
                logger.Error("请设定参数：tcpListenerPort的值。");
                isError = true;
            }
            if (ReadingInterval == null)
            {
                logger.Error("请设定参数：readInterval的值。");
                isError = true;
            }
            if (AnalyseInterval == null)
            {
                logger.Error("请设定参数：analyseInterval的值。");
                isError = true;
            }
            
            return isError;
        }

        #endregion
    }
}
