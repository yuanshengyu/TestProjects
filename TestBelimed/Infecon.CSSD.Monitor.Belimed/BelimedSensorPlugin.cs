using System;
using System.Collections;
using Infecon.Common.COM;
using log4net;
using Quartz;
using Quartz.Spi;

namespace Infecon.CSSD.Monitor.Belimed
{
    /// <summary>
    /// 插件：读取Belimed传感器数据
    /// </summary>
    public class BelimedSensorPlugin : ISchedulerPlugin
    {
        // 日志记录
        protected readonly ILog logger;

        private const string cPluginName = "ReadSensorDataPlugin";

        private const string cJobIdSensorDataRead = "JobId_SensorDataRead";
        private const string cTriggerIdSensorDataRead = "TriggerId_SensorDataRead";

        private const string cJobIdSensorDataAnalyse = "JobId_SensorDataAnalyse";
        private const string cTriggerIdSensorDataAnalyse = "TriggerId_SensorDataAnalyse";

        private const string cJobIdMatchCSSDRecord = "JobId_MatchCSSDRecord";
        private const string cTriggerIdMatchCSSDRecord = "TriggerId_MatchCSSDRecord";

        // 插件名称
        private string mName;
        // 调度器
        private IScheduler mSched;

        /// <summary>
        /// 命名管道句柄
        /// </summary>
        private IntPtr mPipeHandle;

        /// <summary>
        /// 参数：设备组
        /// </summary>
        public string SensorGroup
        {
            set;
            get;
        }

        /// <summary>
        /// 参数：Belimed命名管道服务器的网络名称或IP地址
        /// </summary>
        public string NetworkName
        {
            set;
            get;
        }

        /// <summary>
        /// 参数：Belimed命名管道服务器的数据库名称
        /// </summary>
        public string DatabaseName
        {
            set;
            get;
        }

        /// <summary>
        /// 参数：读取命名管道的时间间隔（单位：毫秒）
        /// </summary>
        [TimeSpanParseRule(TimeSpanParseRule.Milliseconds)]
        public TimeSpan ReadInterval
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

        ///// <summary>
        ///// 参数：匹配CSSD的时间间隔（单位：秒）
        ///// </summary>
        //[TimeSpanParseRule(TimeSpanParseRule.Seconds)]
        //public TimeSpan MatchInterval
        //{
        //    get;
        //    set;
        //}

        public BelimedSensorPlugin()
        {
            logger = LogManager.GetLogger(GetType());
        }

        #region ISchedulerPlugin 成员

        public void Initialize(string pluginName, Quartz.IScheduler sched)
        {
            logger.Info("BelimedSensorPlugin 开始初始化");
            mName = pluginName;
            mSched = sched;
            logger.Info("BelimedSensorPlugin 初始化结束");
        }

        public void Shutdown()
        {
            logger.Info("关闭命名管道连接……");
            bool ret = NamedPipeHelper.CloseNamedPipe(mPipeHandle);
            if (ret)
            {
                logger.Info("成功关闭命名管道连接。");
            }
            else
            {
                logger.Error("关闭命名管道连接失败。");
            }
        }

        public void Start()
        {
            logger.Info("启动传感器监控插件……");

            // 参数检验
            if (CheckParams())
            {
                throw new Exception("插件的参数设定不正确。");
            }


            //生成命名管道名称 \\<Hostname Datalogger>\pipe\<dbxxxx>_TloggerMachineData 
            string pipeName = String.Format(@"\\{0}\pipe\{1}_TloggerMachineData", NetworkName, DatabaseName);

            try
            {
                mPipeHandle = NamedPipeHelper.OpenNamedPipe(pipeName);
                if (mPipeHandle.ToInt32() > -1)
                {
                    logger.Info("成功打开命名管道连接。");
                }
                else
                {
                    logger.Error("打开命名管道连接失败。");
                }
            }
            catch (Exception ex)
            {
                logger.Error("打开命名管道连接失败！");
                throw ex;
            }

            ArrayList readedBuf = new ArrayList();

            IJobDetail jobRead = JobBuilder.Create<DataReadJob>()
                    .WithIdentity(String.Format("{0}_{1}", mName, cJobIdSensorDataRead), cPluginName)
                    .Build();
            jobRead.JobDataMap.Put(DataReadJob.PARAM_NAME_PIPE_HANDLE, mPipeHandle);
            jobRead.JobDataMap.Put(DataReadJob.PARAM_NAME_PIPE_NAME, pipeName);
            jobRead.JobDataMap.Put(Consts.C_READED_BUFFER_KEY, readedBuf);

            ITrigger triggerRead = TriggerBuilder.Create()
                .WithIdentity(String.Format("{0}_{1}", mName, cTriggerIdSensorDataRead), cPluginName)
                .StartNow()
                .WithSimpleSchedule(x => x.WithInterval(ReadInterval).RepeatForever())
                .Build();

            IJobDetail jobAnalyse = JobBuilder.Create<DataAnalyseJob>()
                    .WithIdentity(String.Format("{0}_{1}", mName, cJobIdSensorDataAnalyse), cPluginName)
                    .Build();
            jobAnalyse.JobDataMap.Put(Consts.C_READED_BUFFER_KEY, readedBuf);
            jobAnalyse.JobDataMap.Put(Consts.C_SENSOR_GROUP, SensorGroup);

            ITrigger triggerAnalyse = TriggerBuilder.Create()
                .WithIdentity(String.Format("{0}_{1}", mName, cTriggerIdSensorDataAnalyse), cPluginName)
                .StartNow()
                .WithSimpleSchedule(x => x.WithInterval(AnalyseInterval).RepeatForever())
                .Build();

            //IJobDetail jobMatch = JobBuilder.Create<MatchCSSDRecordJob>()
            //        .WithIdentity(String.Format("{0}_{1}", mName, cJobIdMatchCSSDRecord), cPluginName)
            //        .Build();

            //ITrigger triggerMatch = TriggerBuilder.Create()
            //    .WithIdentity(String.Format("{0}_{1}", mName, cTriggerIdMatchCSSDRecord), cPluginName)
            //    .StartNow()
            //    .WithSimpleSchedule(x => x.WithInterval(MatchInterval).RepeatForever())
            //    .Build();

            try
            {
                // 安排运行任务
                mSched.ScheduleJob(jobRead, triggerRead);
                mSched.ScheduleJob(jobAnalyse, triggerAnalyse);
                //mSched.ScheduleJob(jobMatch, triggerMatch);
            }
            catch (SchedulerException e)
            {
                logger.Error("BelimedSensorPlugin 无法调度任务！");
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
            if (string.IsNullOrEmpty(NetworkName))
            {
                logger.Error("请设定参数：networkName的值。");
                isError = true;
            }
            if (string.IsNullOrEmpty(DatabaseName))
            {
                logger.Error("请设定参数：databaseName的值。");
                isError = true;
            }
            if (ReadInterval == null)
            {
                logger.Error("请设定参数：readInterval的值。");
                isError = true;
            }
            if (AnalyseInterval == null)
            {
                logger.Error("请设定参数：analyseInterval的值。");
                isError = true;
            }
            //if (MatchInterval == null)
            //{
            //    logger.Error("请设定参数：matchInterval的值。");
            //    isError = true;
            //}

            return isError;
        }

        #endregion
    }
}
