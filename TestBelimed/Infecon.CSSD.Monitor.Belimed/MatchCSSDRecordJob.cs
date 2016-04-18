using System;
using System.Collections.Generic;
using System.Data;
using System.Transactions;
using Infecon.Common.Utility;
using Infecon.CSSD.Bll.Monitor;
using Infecon.CSSD.Business;
using Infecon.CSSD.Entity;
using Infecon.CSSD.Entity.Monitor;
using log4net;
using Quartz;

namespace Infecon.CSSD.Monitor.Belimed
{
    [DisallowConcurrentExecution]
    class MatchCSSDRecordJob : IJob
    {
        //日志记录
        private readonly ILog logger;

        /// <summary>
        /// 配置文件路径
        /// </summary>
        //string SettingFilePath = string.Empty;

        /// <summary>
        /// 灭菌、清洗设备
        /// </summary>
        //List<MonitorDeviceEntity> ltMonitorDevice = null;

        public MatchCSSDRecordJob()
        {
            logger = LogManager.GetLogger(GetType());

            //SettingFilePath = GetExePath() + "\\LocalSetting.xml";

            //LoadLocalDevice();
        }

        //private string GetExePath()
        //{
        //    string assemblyFilePath = Assembly.GetExecutingAssembly().Location;
        //    string assemblyDirPath = Path.GetDirectoryName(assemblyFilePath);
        //    return assemblyDirPath;
        //}

        /// <summary>
        /// 获取配置文件
        /// </summary>
        /// <returns></returns>
        //private XmlDocument GetXml()
        //{
        //    try
        //    {
        //        XmlDocument xmlDoc = new XmlDocument();
        //        xmlDoc.Load(SettingFilePath);
        //        return xmlDoc;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("设备配置信息加载失败，不能启动监控程序。");
        //        throw ex;
        //    }


        //}

        /// <summary>
        /// 加载设备配置信息
        /// </summary>
        //private void LoadLocalDevice()
        //{
        //    try
        //    {
        //        XmlDocument xmlDoc = GetXml();
        //        if (xmlDoc == null)
        //        {
        //            throw new Exception("设备配置信息加载失败，不能启动监控程序。");
        //        }

        //        if (ltMonitorDevice == null) ltMonitorDevice = new List<MonitorDeviceEntity>();

        //        //灭菌设备
        //        XmlNode root = xmlDoc.SelectSingleNode("config/DeviceList/SterilizeList");
        //        //if (ltSterilize == null) ltSterilize = new List<MonitorDeviceEntity>();
        //        MonitorDeviceEntity o = null;

        //        foreach (XmlNode n in root.ChildNodes)
        //        {
        //            XmlElement el = (XmlElement)n;

        //            o = (MonitorDeviceEntity)Serializer.DeserializeXmlString(typeof(MonitorDeviceEntity), el.OuterXml);
        //            if (ltMonitorDevice.Exists(d => d.Code == o.Code)) continue;
        //            ltMonitorDevice.Add(o);
        //        }

        //        //清洗设备
        //        root = xmlDoc.SelectSingleNode("config/DeviceList/DisinfectList");
        //        //if (ltDisinfect == null) ltDisinfect = new List<MonitorDeviceEntity>();
        //        foreach (XmlNode n in root.ChildNodes)
        //        {
        //            XmlElement el = (XmlElement)n;

        //            o = (MonitorDeviceEntity)Serializer.DeserializeXmlString(typeof(MonitorDeviceEntity), el.OuterXml);
        //            if (ltMonitorDevice.Exists(d => d.Code == o.Code)) continue;
        //            ltMonitorDevice.Add(o);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        #region IJob 成员
        /// <summary>
        /// 执行任务
        /// </summary>
        /// <param name="context"></param>
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                logger.Info("MatchCSSDRecordJob 任务开始运行");

                MonitorDeviceBll bllMonitorDevice = new MonitorDeviceBll();
                List<QueryParameter> lstParameter = new List<QueryParameter>();
                DataTable dtDevice = bllMonitorDevice.GetMonitorDeviceList(lstParameter);

                foreach (DataRow row in dtDevice.Rows)
                {
                    MonitorDeviceEntity entity = bllMonitorDevice.GetEntityByRow(row);
                    MatchMonitorRecord(entity);
                }

                //foreach (MonitorDeviceEntity dv in ltMonitorDevice)
                //{
                //    MatchMonitorRecord(dv);
                //}

                logger.Info("MatchCSSDRecordJob 任务运行结束");
            }
            catch (Exception ex)
            {
                logger.Error("MatchCSSDRecordJob 运行异常", ex);
            }
        }

        #endregion

        /// <summary>
        /// 匹配CSSD记录
        /// </summary>
        /// <param name="dv"></param>
        private void MatchMonitorRecord(MonitorDeviceEntity dv)
        {
            try
            {
                MonitorDeviceQueueEntity MonitorEntity = null;
                MonitorCollectDataEntity CollectEntity = null;
                MonitorDeviceEntity Device = dv;
                MonitorHelper monitor = new MonitorHelper();

                MonitorEntity = (MonitorDeviceQueueEntity)monitor.SelectSingle(typeof(MonitorDeviceQueueEntity),
                    String.Format("FDeviceID='{0}'", Device.Code), "FBeginDate Desc");

                if (MonitorEntity != null)
                {
                    DateTime deBegin = MonitorEntity.FBeginDate.Value.AddMinutes(-5);
                    DateTime deEnd = MonitorEntity.FBeginDate.Value.AddMinutes(20);

                    #region 队列表中有记录处理

                    CollectEntity = (MonitorCollectDataEntity)monitor.SelectSingle(typeof(MonitorCollectDataEntity),
                        string.Format("FLogID='{0}' And FDeviceID='{1}' And FMonitorType={2}", MonitorEntity.FLogID,
                        MonitorEntity.FDeviceID, Device.DeviceType), string.Empty);

                    if (CollectEntity != null)
                    {
                        //正常情况处理：正常情况是指先在PC系统中做灭菌记录，然后开始启动灭菌设备.
                        if (CollectEntity.FEndDate != null)
                            monitor.Delete(MonitorEntity);
                        logger.Info("正常情况匹配完成");
                    }
                    else
                    {
                        //非正常情况处理
                        CollectEntity = monitor.SelectSingle("FDeviceID='" + MonitorEntity.FDeviceID + "' And FBeginDate>='" +
                            deBegin.ToString("yyyy-MM-dd HH:mm:ss") + "' And FBeginDate<='" +
                            deEnd.ToString("yyyy-MM-dd HH:mm:ss") + "'",
                            "FBeginDate Desc");

                        //CollectEntity = (MonitorCollectDataEntity)monitor.SelectSingle(typeof(MonitorCollectDataEntity),
                        //    string.Format("FDeviceID='{0}' And FMonitorType={1}",
                        //    MonitorEntity.FDeviceID, Device.DeviceType), "FBeginDate Desc");

                        //如果有记录，取时间最近的与当前灭菌记录匹配，并更新FLogID
                        if (CollectEntity == null)
                        {
                            logger.WarnFormat("非正常情况：在时间区间【-5~20】内，未找到监控主数据！(设备ID：[{0}]; 开始时间：[{1}]; 结束时间：[{2}])",
                                MonitorEntity.FDeviceID, deBegin.ToString("yyyy-MM-dd HH:mm:ss"), deEnd.ToString("yyyy-MM-dd HH:mm:ss"));
                            //CollectEntity = new MonitorCollectDataEntity();
                            //CollectEntity.FID = monitor.GetMaxIDByEntity(typeof(MonitorCollectDataEntity));
                            //CollectEntity.FLogID = MonitorEntity.FLogID; CollectEntity.FDeviceID = MonitorEntity.FDeviceID;
                            //CollectEntity.FMonitorType = Device.DeviceType;
                            //CollectEntity.FDataSourceType = Device.SourceType;
                            //CollectEntity.FBeginDate = MonitorEntity.FBeginDate;
                            //monitor.Create(CollectEntity);
                        }
                        else
                        {
                            using (TransactionScope tran = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 5, 0)))
                            {
                                CollectEntity.FLogID = MonitorEntity.FLogID;
                                monitor.Update(CollectEntity);

                                DevicesUseLogEntity uselog = (DevicesUseLogEntity)monitor.SelectSingle(typeof(DevicesUseLogEntity),
                                    string.Format("LogID='{0}'", MonitorEntity.FLogID), string.Empty);
                                if (uselog != null)
                                {
                                    uselog.MonitorDataID = CollectEntity.FID;

                                    if (CollectEntity.FBeginDate != null && CollectEntity.FBeginDate.HasValue)
                                    {
                                        uselog.OldBeginDate = uselog.BeginDate;
                                        uselog.BeginDate = CollectEntity.FBeginDate.Value;
                                    }
                                    uselog.EndDate = CollectEntity.FEndDate;

                                    monitor.Update(uselog);
                                }

                                tran.Complete();
                            }
                            logger.InfoFormat("非正常情况匹配成功，MonitorDeviceQueueEntity.FlogID： [{0}]", MonitorEntity.FLogID);
                        }
                    }

                    #endregion
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                logger.Error(dv);
                throw ex;
            }
        }
    }
}
