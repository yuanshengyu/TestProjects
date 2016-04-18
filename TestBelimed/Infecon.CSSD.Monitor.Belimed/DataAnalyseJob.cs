using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using Infecon.Common.Utility;
using Infecon.CSSD.Business;
using Infecon.CSSD.Entity;
using Infecon.CSSD.Entity.Monitor;
using Infecon.CSSD.Utility;
using log4net;
using Quartz;
using Infecon.CSSD.Bll.Sensor;
using Infecon.CSSD.Entity.Sensor;
using Infecon.CSSD.Business.Monitor;

namespace Infecon.CSSD.Monitor.Belimed
{
    /// <summary>
    /// 解析传感器数据
    /// </summary>
    [PersistJobDataAfterExecution]
    class DataAnalyseJob : IJob
    {
        //日志记录
        protected readonly ILog logger;

        private string mSensorGroup;

        public DataAnalyseJob()
        {
            logger = LogManager.GetLogger(GetType());
        }

        #region IJob 成员

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                logger.Info("DataAnalyseJob 任务开始运行");

                ArrayList readedBuf = (ArrayList)context.MergedJobDataMap.Get(Consts.C_READED_BUFFER_KEY);//"ReadedBuffer"

                mSensorGroup = (string)context.MergedJobDataMap.Get(Consts.C_SENSOR_GROUP);//"SensorGroup"

                if (readedBuf.Count > 0)
                {
                    logger.Info("开始解析……");

                    IList<byte> lstTodoBytes = SplitBuffer(readedBuf);

                    if (lstTodoBytes.Count > 0)
                    {
                        IList<IList<byte>> lstBytesRow = ChangeToRows(lstTodoBytes);
                        logger.Debug("分拆成行的Byte数据：");
                        foreach (IList<byte> row in lstBytesRow)
                        {
                            logger.Debug(ToolHelper.ByteArrayToHexString(row.ToArray()));
                        }
                        IList<IList<string>> lstStringRow = AnalyseData(lstBytesRow);
                        logger.Debug("解析后的行数据：");
                        foreach (IList<string> row in lstStringRow)
                        {
                            logger.Debug(row);
                        }

                        if (lstStringRow.Count > 0)
                        {
                            // 解析后的数据存入数据库

                            for(int i = 0; i < lstStringRow.Count; i++)
                            {
                                IList<string> row = lstStringRow[i];

                                string msgType = row[0].Trim();

                                string strByteData = string.Empty;
                                string strData = string.Empty;
                                foreach (byte b in lstBytesRow[i])
                                {
                                    strByteData += b.ToString() + " ";
                                }
                                foreach (string str in row)
                                {
                                    strData += str + ";";
                                }

                                switch (msgType)
                                {
                                    case "1":
                                        // Message "System Data"
                                        logger.Debug("System Data（原始）: " + strByteData);
                                        logger.Debug("System Data（转换）: " + strData);
                                        AnalyseData(row, strByteData);
                                        break;
                                    case "2":
                                        // Message "Batch Data"
                                        logger.Warn("Batch Data（原始）: " + strByteData);
                                        logger.Warn("Batch Data（转换）: " + strData);
                                        break;
                                    case "3":
                                        // Message "Batch Data INT"
                                        logger.Warn("Batch Data INT（原始）: " + strByteData);
                                        logger.Warn("Batch Data INT（转换）: " + strData);
                                        break;
                                    case "4":
                                        // Message "Batch Parameters CHAR" 
                                        logger.Warn("Batch Parameters CHAR（原始）: " + strByteData);
                                        logger.Warn("Batch Parameters CHAR（转换）: " + strData);
                                        break;
                                    default:
                                        // 未定义 信息类型
                                        logger.Warn("未定义信息（原始）: " + strByteData);
                                        logger.Warn("未定义信息（转换）: " + strData);
                                        break;
                                }
                            }
                        }
                    }

                    logger.Info("解析完成。");
                }


                logger.Info("DataAnalyseJob 任务运行结束");
            }
            catch (Exception ex)
            {
                logger.Error("DataAnalyseJob 运行异常", ex);
                throw ex;
            }
        }

        private void AnalyseData(IList<string> row, string strByteData)
        {
            string sysNo = ParseHelper.ParseString(row[(int)SystemData.SystemNo]);
            string batchNo = ParseHelper.ParseString(row[(int)SystemData.BatchNo]);
            DateTime timestamp = ConvertDate(row[(int)SystemData.Timestamp]);

            SensorHelper<object> helperSensor = new SensorHelper<object>();
            SensorEntity eSensor = helperSensor.SelectSingle<SensorEntity>(string.Format("SensorGroup = '{0}' AND SensorKey = '{1}'", mSensorGroup, sysNo), string.Empty);

            if (eSensor == null)
            {
                logger.WarnFormat("系统中组和编号为：[{0}].[{1}]的设备没有登记。", mSensorGroup, sysNo);
                return;
            }

            SensorDataHeadEntity eHead = helperSensor.SelectSingle<SensorDataHeadEntity>(string.Format("SensorID = '{0}' AND DataKey = '{1}'", eSensor.SensorID.ToString().ToUpper(), batchNo), string.Empty);

            Enumerator.BelimedStatus status;//0-Off,1-On,2-Run,3-End
            try
            {
                status = (Enumerator.BelimedStatus)Enum.Parse(typeof(Enumerator.BelimedStatus), row[(int)SystemData.Status]);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("传感器状态取得失败，状态值：[{0}]", row[(int)SystemData.Status]);
                throw ex;
            }

            System.Threading.Monitor.Enter(Monitor.Common.Utility.SyncSqlLock);
            try
            {
                logger.Info("开始保存数据。");
                using (TransactionScope tran = Monitor.Common.Utility.GetTransactionScope())
                {
                    switch (status)
                    {
                        case Enumerator.BelimedStatus.Off:
                            eSensor.PositionMark = status.ToString();
                            helperSensor.Update(eSensor);
                            break;
                        case Enumerator.BelimedStatus.On:
                            eSensor.PositionMark = status.ToString();
                            helperSensor.Update(eSensor);
                            break;
                        case Enumerator.BelimedStatus.Run:
                            if (eHead == null)
                            {
                                // 主数据
                                eHead = new SensorDataHeadEntity();
                                eHead.SensorDataHeadID = Guid.NewGuid();
                                eHead.SensorID = eSensor.SensorID;
                                eHead.DataKey = batchNo;
                                eHead.BeginDate = timestamp;
                                eHead.IsCompressed = false;
                                eHead.DataVer = DataVersion.V1.ToString();

                                helperSensor.Create(eHead);
                            }

                            eSensor.PositionMark = status.ToString();
                            helperSensor.Update(eSensor);

                            // 原始数据
                            SensorDataRawLineEntity eRawLine = new SensorDataRawLineEntity();
                            eRawLine.SensorDataRawLineID = Guid.NewGuid();
                            eRawLine.SensorDataHeadID = eHead.SensorDataHeadID;
                            eRawLine.ReceivedDate = DateTime.Now;
                            eRawLine.RawData = strByteData;
                            helperSensor.Create(eRawLine);

                            // 解析数据
                            SensorDataLineEntity eLine = new SensorDataLineEntity();
                            eLine.SensorDataLineID = Guid.NewGuid();
                            eLine.SensorDataHeadID = eHead.SensorDataHeadID;
                            eLine.SensorDataRawLineID = eRawLine.SensorDataRawLineID;
                            eLine.ReceivedDate = eRawLine.ReceivedDate;
                            eLine.SensorDate = timestamp;
                            if (eLine.SensorDate != null && eLine.SensorDate.HasValue && eHead.BeginDate != null && eHead.BeginDate.HasValue)
                            {
                                eLine.SensorTimeSpan = Convert.ToInt64(eLine.SensorDate.Value.Subtract(eHead.BeginDate.Value).TotalMilliseconds);
                            }
                            //eLine.AnalysedData = row.ToString();
                            eLine.AnalysedData = string.Join(";", row.ToArray<string>());
                            helperSensor.Create(eLine);

                            SensorDataLineValueEntity eLineValue = null;
                            for (int i = 0; i < row.Count; i++)
                            {
                                eLineValue = new SensorDataLineValueEntity();
                                eLineValue.SensorDataLineValueID = Guid.NewGuid();
                                eLineValue.SensorDataHeadID = eLine.SensorDataHeadID;
                                eLineValue.SensorDataLineID = eLine.SensorDataLineID;

                                switch (i)
                                {
                                    case (int)SystemData.Status:
                                        string ss = ParseHelper.ParseString(row[i]);
                                        if (!string.IsNullOrEmpty(ss))
                                        {
                                            eLineValue.ValueType = SystemData.Status.ToString();
                                            eLineValue.ContentString = ss;
                                            helperSensor.Create(eLineValue);
                                        }
                                        break;
                                    case (int)SystemData.ErrorNo:
                                        string errorNo = ParseHelper.ParseString(row[i]);
                                        if (!string.IsNullOrEmpty(errorNo))
                                        {
                                            eLineValue.ValueType = SystemData.ErrorNo.ToString();
                                            eLineValue.ContentString = errorNo;
                                            helperSensor.Create(eLineValue);
                                        }
                                        break;
                                    case (int)SystemData.ProgramNo:
                                        eLineValue.ValueType = SystemData.ProgramNo.ToString();
                                        eLineValue.ContentString = ParseHelper.ParseString(row[i]);
                                        helperSensor.Create(eLineValue);
                                        break;
                                    case (int)SystemData.ProgramName:
                                        eLineValue.ValueType = SystemData.ProgramName.ToString();
                                        eLineValue.ContentString = ParseHelper.ParseString(row[i]);
                                        helperSensor.Create(eLineValue);
                                        break;
                                    case (int)SystemData.ProgramPhase:
                                        eLineValue.ValueType = SystemData.ProgramPhase.ToString();
                                        eLineValue.ContentString = ParseHelper.ParseString(row[i]);
                                        helperSensor.Create(eLineValue);
                                        break;
                                    default:
                                        break;
                                }

                                if (Convert.ToInt32(eSensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Washer)))
                                {
                                    switch (i)
                                    {
                                        case (int)SystemData.MeasurementSensor1:
                                            // Med
                                            eLineValue.ValueType = SystemData.MeasurementSensor1.ToString();
                                            eLineValue.ContentNumber = Convert.ToDecimal(row[i]);
                                            helperSensor.Create(eLineValue);
                                            break;
                                        case (int)SystemData.MeasurementSensor2:
                                            // CT
                                            eLineValue.ValueType = SystemData.MeasurementSensor2.ToString();
                                            eLineValue.ContentNumber = Convert.ToDecimal(row[i]) / 10;
                                            helperSensor.Create(eLineValue);
                                            break;
                                        case (int)SystemData.MeasurementSensor3:
                                            // CDNo
                                            eLineValue.ValueType = SystemData.MeasurementSensor3.ToString();
                                            eLineValue.ContentNumber = Convert.ToDecimal(row[i]);
                                            helperSensor.Create(eLineValue);
                                            break;
                                        case (int)SystemData.MeasurementSensor4:
                                            // DosV
                                            eLineValue.ValueType = SystemData.MeasurementSensor4.ToString();
                                            eLineValue.ContentNumber = Convert.ToDecimal(row[i]) / 10;
                                            helperSensor.Create(eLineValue);
                                            break;
                                        case (int)SystemData.MeasurementSensor5:
                                            // Ao
                                            eLineValue.ValueType = SystemData.MeasurementSensor5.ToString();
                                            eLineValue.ContentNumber = Convert.ToDecimal(row[i]);
                                            helperSensor.Create(eLineValue);
                                            break;

                                        default:
                                            break;
                                    }
                                }
                                else if (Convert.ToInt32(eSensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Sterilizer)))
                                {
                                    switch (i)
                                    {
                                        case (int)SystemData.MeasurementSensor1:
                                            // T1
                                            eLineValue.ValueType = SystemData.MeasurementSensor1.ToString();
                                            eLineValue.ContentNumber = Convert.ToDecimal(row[i]) / 10;
                                            helperSensor.Create(eLineValue);
                                            break;
                                        case (int)SystemData.MeasurementSensor2:
                                            // T2
                                            eLineValue.ValueType = SystemData.MeasurementSensor2.ToString();
                                            eLineValue.ContentNumber = Convert.ToDecimal(row[i]) / 10;
                                            helperSensor.Create(eLineValue);
                                            break;
                                        case (int)SystemData.MeasurementSensor4:
                                            // P1
                                            eLineValue.ValueType = SystemData.MeasurementSensor4.ToString();
                                            eLineValue.ContentNumber = Convert.ToDecimal(row[i]);
                                            helperSensor.Create(eLineValue);
                                            break;
                                        case (int)SystemData.MeasurementSensor5:
                                            // P2
                                            eLineValue.ValueType = SystemData.MeasurementSensor5.ToString();
                                            eLineValue.ContentNumber = Convert.ToDecimal(row[i]);
                                            helperSensor.Create(eLineValue);
                                            break;
                                        case (int)SystemData.MeasurementSensor6:
                                            // P3
                                            eLineValue.ValueType = SystemData.MeasurementSensor6.ToString();
                                            eLineValue.ContentNumber = Convert.ToDecimal(row[i]);
                                            helperSensor.Create(eLineValue);
                                            break;
                                        case (int)SystemData.MeasurementSensor7:
                                            // P4
                                            eLineValue.ValueType = SystemData.MeasurementSensor7.ToString();
                                            eLineValue.ContentNumber = Convert.ToDecimal(row[i]);
                                            helperSensor.Create(eLineValue);
                                            break;
                                        default:
                                            break;
                                    }
                                }

                            }

                            break;
                        case Enumerator.BelimedStatus.End:
                            if (eHead != null)
                            {
                                eHead.EndDate = ConvertDate(row[(int)SystemData.Timestamp]);
                                helperSensor.Update(eHead);

                                eSensor.PositionMark = status.ToString();
                                eSensor.Remarks = ParseHelper.ParseString(row[(int)SystemData.StatusInfo]);
                                helperSensor.Update(eSensor);

                                if (eHead.LogID != null && eHead.LogID.HasValue && eHead.LogID.Value != Guid.Empty)
                                {
                                    DevicesUseLogEntity logEntity = helperSensor.SelectSingle<DevicesUseLogEntity>("LogID='" + eHead.LogID.Value.ToString().ToUpper() + "'", string.Empty);
                                    if (logEntity != null)
                                    {
                                        if (eHead.BeginDate != null && eHead.BeginDate.HasValue)
                                        {
                                            logEntity.OldBeginDate = logEntity.BeginDate;
                                            logEntity.BeginDate = eHead.BeginDate.Value;
                                        }
                                        logEntity.EndDate = eHead.EndDate;
                                        // TODO: DevicesUseLog 关联到 Sensor 的字段
                                        //logEntity.MonitorDataID = eHead.SensorDataHeadID;

                                        helperSensor.Update(logEntity);
                                    }
                                }
                            }

                            break;
                        default:
                            break;
                    }
                    tran.Complete();
                    logger.Info("数据保存成功。");
                }
            }
            finally
            {
                System.Threading.Monitor.Exit(Monitor.Common.Utility.SyncSqlLock);
            } 

        }


        #endregion

        private IList<IList<string>> AnalyseData(IList<IList<byte>> lstRow)
        {
            // 存放解析后的数据
            IList<IList<string>> lstDatas = new List<IList<string>>();
            foreach (IList<byte> line in lstRow)
            {
                IList<string> lstWords = new List<string>();
                IList<byte> lstWord = new List<byte>();

                // TODO:甘肃省人民医院，接收到的数据缺少分隔符
                if ((line.Count > 0x54) && (Convert.ToInt32(line[0x54]) != 0))
                {
                    line.Insert(0x54, 0);
                }

                foreach (byte b in line)
                {
                    if (System.Convert.ToInt32(b) == 0)
                    {
                        if (lstWord.Count > 0)
                        {
                            lstWords.Add(Encoding.UTF8.GetString(lstWord.ToArray()).Trim());
                            lstWord.Clear();
                        }
                    }
                    else
                    {
                        lstWord.Add(b);
                    }
                }
                lstDatas.Add(lstWords);
            }
            return lstDatas;
        }

        private IList<IList<byte>> ChangeToRows(IList<byte> lstTodo)//SplitBuffer返回的lstTodo最多只有一个回车换行
        {
            IList<IList<byte>> lstLines = new List<IList<byte>>();
            IList<byte> lstCols = new List<byte>();
            for (int i = 0; i < lstTodo.Count; i++)
            {
                lstCols.Add(lstTodo[i]);
                if (i > 0 && System.Convert.ToInt32(lstTodo[i - 1]) == 13 && System.Convert.ToInt32(lstTodo[i]) == 10)
                {
                    lstLines.Add(lstCols);
                    lstCols = new List<byte>();
                }
            }
            return lstLines;
        }

        private IList<byte> SplitBuffer(ArrayList readedBuf)
        {
            IList<byte> lstBytes = new List<byte>();
            IList<byte> lstTodo = new List<byte>();     // 本次将解析的数据
            IList<byte> lstNext = new List<byte>();     // 留到下次解析的数据
            lock (readedBuf.SyncRoot)
            {
                foreach (byte[] buf in readedBuf)
                {
                    foreach (byte b in buf)
                    {
                        lstBytes.Add(b);
                    }
                }

                for (int i = lstBytes.Count - 1; i > 0; i--)
                {
                    if (System.Convert.ToInt32(lstBytes[i - 1]) == 13 && System.Convert.ToInt32(lstBytes[i]) == 10)//10换行LF  13回车CR       CRLF
                    {
                        for (int j = 0; j <= i; j++)
                        {
                            lstTodo.Add(lstBytes[j]);
                        }
                        for (int j = i + 1; j < lstBytes.Count; j++)
                        {
                            lstNext.Add(lstBytes[j]);
                        }

                        break;
                    }
                }

                if (lstTodo.Count > 0 || lstNext.Count > 0)
                {
                    readedBuf.Clear();
                }
                if (lstNext.Count > 0)
                {
                    readedBuf.Add(lstNext.ToArray());
                }
            }

            logger.InfoFormat("缓存中的数据个数（byte）：[{0}]", lstBytes.Count);
            logger.InfoFormat("本次处理的数据个数（byte）：[{0}]", lstTodo.Count);
            logger.InfoFormat("留待下次处理的数据个数（byte）：[{0}]", lstNext.Count);

            //lstBytes.Clear();
            //lstNext.Clear();
            return lstTodo;
        }

        /// <summary>
        /// 转换时间
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private DateTime ConvertDate(string date)
        {
            try
            {
                string[] strTemmp = date.Split('-');
                return DateTime.Parse(string.Format("{0}-{1}-{2} {3}",
                    strTemmp));
                //return DateTime.Parse(date.Substring(0, 10) + " " + date.Substring(11, 8));
            }
            catch
            {
                throw new Exception("转换时间出错(error when convert date string).");
            }
        }

    }
}
