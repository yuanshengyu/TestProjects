using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Infecon.Common.Utility;
using log4net;
using Quartz;
using Infecon.CSSD.Entity.Sensor;
using Infecon.CSSD.Business.Monitor;
using System.Data;

namespace Infecon.CSSD.Monitor.SmartNode
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

        SerialPortCmdFormat SerialPortCmdFormat;


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

                ArrayList readedBuf = (ArrayList)context.MergedJobDataMap.Get(Consts.C_READED_BUFFER_KEY);

                mSensorGroup = (string)context.MergedJobDataMap.Get(Consts.C_SENSOR_GROUP_KEY);

                SerialPortCmdFormat = Global.UserProfile.SensorNode.SerialPortCmdFormat;

                if (readedBuf.Count > 0)
                {
                    logger.Info("开始解析……");

                    IList<IList<byte>> lstBytesRow = SplitBuffer(readedBuf);
                    logger.Debug("分拆成行的Byte数据：");
                    foreach (IList<byte> row in lstBytesRow)
                    {
                        logger.Debug(ToolHelper.ByteArrayToHexString(row.ToArray()));
                    }

                    if (lstBytesRow != null && lstBytesRow.Count > 0)
                    {
                        for (int i = 0; i < lstBytesRow.Count; i++)
                        {
                            string hexString = ToolHelper.ByteArrayToHexString(lstBytesRow[i].ToArray());
                            logger.DebugFormat("正在处理数据：", hexString);
                            SensorData sensorData = PraseHexString(hexString, lstBytesRow[i]);
                            if (sensorData != null)
                            {
                                SaveData(sensorData);
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

        #endregion

        private void SaveData(SensorData receivedData)
        {
            SensorHelper<object> helperSensor = new SensorHelper<object>();
            SensorEntity eSensor = helperSensor.SelectSingle<SensorEntity>(string.Format("SensorGroup = '{0}' AND SensorKey = '{1}'", mSensorGroup, receivedData.SourceAddress), string.Empty);
            if (eSensor == null)
            {
                logger.WarnFormat("系统中组和编号为：[{0}].[{1}]的设备没有登记。", mSensorGroup, receivedData.SourceAddress);
                return;
            }

            string dataKey = receivedData.ReceiveDate.Value.ToString("yyyyMMdd");
            SensorDataHeadEntity eHead = helperSensor.SelectSingle<SensorDataHeadEntity>(string.Format("SensorID = '{0}' AND DataKey = '{1}'", eSensor.SensorID.ToString().ToUpper(), dataKey), string.Empty);

            System.Threading.Monitor.Enter(Monitor.Common.Utility.SyncSqlLock);
            try
            {
                logger.Debug("开始保存数据。");
                using (TransactionScope tran = Monitor.Common.Utility.GetTransactionScope())
                {
                    if (eHead == null)
                    {
                        // 主数据
                        eHead = new SensorDataHeadEntity();
                        eHead.SensorDataHeadID = Guid.NewGuid();
                        eHead.SensorID = eSensor.SensorID;
                        eHead.DataKey = dataKey;
                        eHead.BeginDate = receivedData.ReceiveDate;
                        eHead.IsCompressed = false;

                        helperSensor.Create(eHead);
                    }

                    // 原始数据
                    SensorDataRawLineEntity eRawLine = new SensorDataRawLineEntity();
                    eRawLine.SensorDataRawLineID = Guid.NewGuid();
                    eRawLine.SensorDataHeadID = eHead.SensorDataHeadID;
                    eRawLine.ReceivedDate = receivedData.ReceiveDate;
                    eRawLine.RawData = receivedData.SourceData;
                    helperSensor.Create(eRawLine);

                    // 明细数据
                    SensorDataLineEntity eLine = new SensorDataLineEntity();
                    eLine.SensorDataLineID = Guid.NewGuid();
                    eLine.SensorDataHeadID = eHead.SensorDataHeadID;
                    eLine.SensorDataRawLineID = eRawLine.SensorDataRawLineID;
                    eLine.ReceivedDate = eRawLine.ReceivedDate;
                    eLine.SensorDate = eRawLine.ReceivedDate;
                    if (eLine.SensorDate != null && eLine.SensorDate.HasValue && eHead.BeginDate != null && eHead.BeginDate.HasValue)
                    {
                        eLine.SensorTimeSpan = Convert.ToInt64(eLine.SensorDate.Value.Subtract(eHead.BeginDate.Value).TotalMilliseconds);
                    }
                    //eLine.AnalysedData = string.Empty;
                    eLine.ValueNumber1 = receivedData.Temperature;
                    eLine.ValueNumber2 = receivedData.Pressure;
                    eLine.ValueNumber3 = receivedData.Humidity;
                    eLine.ValueNumber4 = receivedData.Voltage;
                    helperSensor.Create(eLine);

                    tran.Complete();
                    logger.Debug("数据保存成功。");
                }
            }
            finally
            {
                System.Threading.Monitor.Exit(Monitor.Common.Utility.SyncSqlLock);
            } 
        }

        private IList<IList<byte>> SplitBuffer(ArrayList readedBuf)
        {
            IList<byte> lstBytes = new List<byte>();
            IList<byte> lstNext = new List<byte>();     // 留到下次解析的数据

            IList<IList<byte>> lstLines = new List<IList<byte>>();

            int startIndex = 0;

            lock (readedBuf.SyncRoot)
            {
                foreach (byte[] buf in readedBuf)
                {
                    foreach (byte b in buf)
                    {
                        lstBytes.Add(b);
                    }
                }


                IList<byte> lstCols = null;
                do
                {
                    lstCols = SplitToLine(lstBytes, ref startIndex);
                    if (lstCols != null)
                    {
                        lstLines.Add(lstCols);
                    }
                } while (lstCols != null);

                for (int i = startIndex; i < lstBytes.Count; i++)
                {
                    lstNext.Add(lstBytes[i]);
                }

                readedBuf.Clear();
                if (lstNext.Count > 0)
                {
                    readedBuf.Add(lstNext.ToArray());
                }

                
            }

            logger.InfoFormat("缓存中的数据个数（byte）：[{0}]", lstBytes.Count);
            //logger.InfoFormat("本次处理的数据个数（byte）：[{0}]", lstTodo.Count);
            logger.InfoFormat("留待下次处理的数据个数（byte）：[{0}]", lstNext.Count);

            //lstBytes.Clear();
            //lstNext.Clear();
            return lstLines;
        }

        private IList<byte> SplitToLine(IList<byte> lstBytes, ref int startIndex)
        {
            int syncIndex = -1;
            int stxIndex = -1;
            int dataLen = 0;

            int endIndex = 0;

            IList<byte> lstCols = null;
            for (int i = startIndex; i < lstBytes.Count - 1; i++)
            {
                if (lstBytes[i] == 0xAA)
                {
                    // 同步标识
                    syncIndex = i;
                }
                else
                {
                    if (syncIndex != -1)
                    {
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            if (syncIndex > -1)
            {
                if (syncIndex + 1 < lstBytes.Count)
                {
                    if (lstBytes[syncIndex + 1] == 0x75)
                    {
                        // 起始标识
                        stxIndex = syncIndex + 1;

                        if (stxIndex + 1 < lstBytes.Count)
                        {
                            // 数据长度(包括同步，起始标识)
                            dataLen = lstBytes[stxIndex + 1] + 2;

                            endIndex = syncIndex + dataLen - 1;
                            if (endIndex < lstBytes.Count)
                            {
                                lstCols = new List<byte>();
                                for (int j = syncIndex; j <= endIndex; j++)
                                {
                                    lstCols.Add(lstBytes[j]);
                                }

                                startIndex = endIndex + 1;
                                return lstCols;
                            } //if (syncIndex + dataLen - 1 < lstBytes.Count)
                            else
                            {
                                // 接收的数据不够，留待下次处理
                                startIndex = syncIndex;
                            }
                        } //if (stxIndex + 1 < lstBytes.Count)
                        else
                        {
                            // 接收的数据不够，留待下次处理
                            startIndex = syncIndex;
                        }
                    } //if (lstBytes[syncIndex + 1] == 0x75)
                    else
                    {
                        // 接收的数据有问题，非（同步标识+起始标识）
                        logger.WarnFormat("同步起始标识识别错误：同步[{0}]，起始[{1}]\n数据：\n", ToolHelper.ByteArrayToHexString(lstBytes.ToArray()));
                        // 略过当前的同步标识
                        startIndex = syncIndex + 1;
                    }
                } //if (syncIndex + 1 < lstBytes.Count)
                else
                {
                    // 接收的数据不够，留待下次处理
                    startIndex = syncIndex;
                }
            }
            else
            {
                // 无法找到同步标识，忽略所有数据
                startIndex = lstBytes.Count;
            }

            return null;
        }


        #region PraseHexString
        /// <summary>
        /// PraseHexString
        /// 16进制数据处理
        /// 根据数据进行解析.
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        private SensorData PraseHexString(string hexString, IList<byte> rowByte)
        {
            SensorData data = new SensorData();
            data.SourceData = hexString;
            data.ReceiveDate = DateTime.Now;
            data.CMD = hexString.Substring(this.SerialPortCmdFormat.CMD.StartBit, this.SerialPortCmdFormat.CMD.Bits);

            data.SourceAddress = hexString.Substring(this.SerialPortCmdFormat.SourceAddress.StartBit, this.SerialPortCmdFormat.SourceAddress.Bits);

            SensorHelper<object> helper = new SensorHelper<object>();
            SensorEntity eSensor = null;
            if (!string.IsNullOrEmpty(data.SourceAddress))
            {
                eSensor = helper.SelectSingle<SensorEntity>(string.Format("SensorGroup = '{0}' AND SensorKey = '{1}'", mSensorGroup, data.SourceAddress), string.Empty);
            }
            else
            {
                logger.ErrorFormat("接收到的数据中，源地址取得失败。\n数据：{0}", hexString);
                return null;
            }

            if (eSensor == null)
            {
                logger.WarnFormat("系统中组和编号为：[{0}].[{1}]的设备没有登记。", mSensorGroup, data.SourceAddress);
                return null;
            }

            if (eSensor.SensorModel == SensorModel.SD1HT.ToString())
            {
                // 型号：SD1HT
                // 0  1  2  3  4  5  6  7  8  9  10 11 12 13 14 15 16 17
                // AA 75 10 62 00 01 7D C5 A9 AC 0A B0 62 14 1B 96 00 00 
                //             ----------- -------- -------- -----
                //             源地址      温度     湿度     电压
                // 温度：A8（正常温度），A9（温度超上限时的温度），AA（温度超下限是的温度），AB（温度变化过快时的温度）
                // 湿度：B0（正常湿度），B1（湿度超上限时的湿度），B2（湿度超下限是的湿度），B3（湿度变化过快时的湿度）
                // 电压：1B（正常电压），1A（电压低于电压下限阀值时的电压）
                byte[] tempBtye = null;
                decimal temp;
                // 温度
                if (rowByte[8] == 0xA8 || rowByte[8] == 0xA9 || rowByte[8] == 0xAA || rowByte[8] == 0xAB || rowByte[8] == 0xB0)
                {
                    tempBtye = new byte[] { rowByte[9], rowByte[10] };
                    temp = BitConverter.ToUInt16(tempBtye, 0);
                    data.Temperature = temp / 100;
                }
                else
                {
                    logger.ErrorFormat("接收到的数据中，温度取得失败。\n数据：{0}", hexString);
                    return null;
                }

                // 湿度
                if (rowByte[11] == 0xB0 || rowByte[11] == 0xB1 || rowByte[11] == 0xB2 || rowByte[11] == 0xB3 || rowByte[11] == 0xB8)
                {
                    tempBtye = new byte[] { rowByte[12], rowByte[13] };
                    temp = BitConverter.ToUInt16(tempBtye, 0);
                    data.Humidity = temp / 100;
                }
                else
                {
                    logger.ErrorFormat("接收到的数据中，湿度取得失败。\n数据：{0}", hexString);
                    return null;
                }

                // 电压
                if (rowByte[14] == 0x1B || rowByte[14] == 0x1A)
                {
                    tempBtye = new byte[] { rowByte[15], 0 };
                    temp = BitConverter.ToUInt16(tempBtye, 0);
                    data.Voltage = temp / 50;
                }
                else
                {
                    logger.ErrorFormat("接收到的数据中，电压取得失败。\n数据：{0}", hexString);
                    return null;
                }


            }
            else if (eSensor.SensorModel == SensorModel.TH80.ToString())
            {
                // 型号：TH80
                // 0  1  2  3  4  5  6  7  8  9  10 11 12 13 14 15 16 17
                // AA 75 10 62 00 01 B9 FC B0 87 05 B8 25 0D 1B 97 00 00
                //             ----------- -------- -------- -----
                //             源地址      温度     湿度     电压
                // 第一路温湿度参数
                // 温度：B0（当前温度），B1（温度上限），B2（温度下限），B3（温度变化）
                // 湿度：B8（当前湿度），B9（湿度上限），BA（湿度下限），BB（湿度变化）
                // 电压：1B（正常电压），1A（电压低于电压下限阀值时的电压）
                byte[] tempBtye = null;
                decimal temp;
                // 温度
                if (rowByte[8] == 0xB0 || rowByte[8] == 0xB1 || rowByte[8] == 0xB2 || rowByte[8] == 0xB3)
                {
                    tempBtye = new byte[] { rowByte[9], rowByte[10] };
                    temp = BitConverter.ToUInt16(tempBtye, 0);
                    data.Temperature = temp / 100;
                }
                else
                {
                    logger.ErrorFormat("接收到的数据中，温度取得失败。\n数据：{0}", hexString);
                    return null;
                }

                // 湿度
                if (rowByte[11] == 0xB8 || rowByte[11] == 0xB9 || rowByte[11] == 0xBA || rowByte[11] == 0xBB)
                {
                    tempBtye = new byte[] { rowByte[12], rowByte[13] };
                    temp = BitConverter.ToUInt16(tempBtye, 0);
                    data.Humidity = temp / 100;
                }
                else
                {
                    logger.ErrorFormat("接收到的数据中，湿度取得失败。\n数据：{0}", hexString);
                    return null;
                }

                // 电压
                if (rowByte[14] == 0x1B || rowByte[14] == 0x1A)
                {
                    tempBtye = new byte[] { rowByte[15], 0 };
                    temp = BitConverter.ToUInt16(tempBtye, 0);
                    data.Voltage = temp / 50;
                }
                else
                {
                    logger.ErrorFormat("接收到的数据中，电压取得失败。\n数据：{0}", hexString);
                    return null;
                }


            }
            else
            {


                Payload payload = this.SerialPortCmdFormat.Payloads[data.SourceAddress];
                if (payload == null)
                {
                    //说明该节点未配置在 UserProfile.config中
                    logger.WarnFormat("请在 UserProfile.config 的 MatchSensorNodes 节中加入传感器[{0}]的信息。", data.SourceAddress);
                    return null;
                }

                data.Payload = hexString.Substring(payload.StartBit, payload.Bits);
                //data.Payload = data.Payload.Substring(0, 12);

                int index = -1;

                if (payload.TemperatureSymbol != null)
                {
                    //index = data.Payload.IndexOf(payload.TemperatureSymbol.DataDefine);
                    index = payload.TemperatureSymbol.StartBit;
                    if (index != -1)
                    {
                        if (hexString.Substring(22, 2) != "1B")
                        {
                            string Temperature = data.Payload.Substring((index + payload.TemperatureSymbol.Bits), payload.TemperatureData.Bits);
                            ////反转Payload数据, 原始数据 低位在前  高位在后
                            ////payload 数据格式 EB 02  EB是第一位 02是第二位 因此转成Hex进制时需要反转成0x02EB
                            Temperature = Temperature.Substring(2, 2) + Temperature.Substring(0, 2);
                            ////公式算法
                            Temperature = Convert.ToInt64("0x" + Temperature, 16).ToString();

                            //MSScriptControl.ScriptControl sc = new MSScriptControl.ScriptControlClass();
                            //sc.Language = "JavaScript";
                            Temperature = string.Format(payload.TemperatureData.Expression, Temperature);
                            //m_parser.SetExpr(Temperature);
                            //Temperature = m_parser.Eval().ToString();

                            Temperature = new DataTable().Compute(Temperature, "").ToString();
                            //Temperature = Microsoft.JScript.GlobalObject.eval(Temperature).ToString();
                            //Temperature = sc.Eval(Temperature).ToString();
                            data.Temperature = Decimal.Parse(Temperature);
                        }
                        else
                        {
                            string Temperature = data.Payload.Substring((index + payload.TemperatureSymbol.Bits), payload.TemperatureData.Bits);
                            ////反转Payload数据, 原始数据 低位在前  高位在后
                            ////payload 数据格式 EB 02  EB是第一位 02是第二位 因此转成Hex进制时需要反转成0x02EB
                            Temperature = Temperature.Substring(2, 2) + Temperature.Substring(0, 2);
                            ////公式算法
                            Temperature = Convert.ToInt64("0x" + Temperature, 16).ToString();

                            //MSScriptControl.ScriptControl sc = new MSScriptControl.ScriptControlClass();
                            //sc.Language = "JavaScript";
                            //Temperature = string.Format(payload.TemperatureData.Expression, Temperature);
                            //m_parser.SetExpr(Temperature);
                            //Temperature = m_parser.Eval().ToString();

                            //Temperature = new DataTable().Compute(Temperature, "").ToString();
                            //Temperature = Microsoft.JScript.GlobalObject.eval(Temperature).ToString();
                            //Temperature = sc.Eval(Temperature).ToString();
                            data.Temperature = (decimal)((int.Parse(Temperature) - 500) / 10);
                        }
                    }
                }

                if (payload.HumiditySymbol != null)
                {
                    //index = data.Payload.IndexOf(this.SerialPortCmdFormat.Payload.HumiditySymbol.DataDefine);
                    //if (index != -1)
                    index = payload.HumiditySymbol.StartBit;
                    if (index != -1)
                    {
                        if (hexString.Substring(22, 2) != "1B")
                        {
                            string Humidity = data.Payload.Substring((index + payload.HumiditySymbol.Bits), payload.HumidityData.Bits);
                            ////反转Payload数据, 原始数据 低位在前  高位在后
                            ////payload 数据格式 EB 02  EB是第一位 02是第二位 因此转成Hex进制时需要反转成0x02EB
                            Humidity = Humidity.Substring(2, 2) + Humidity.Substring(0, 2);
                            ////公式算法
                            Humidity = Convert.ToInt64("0x" + Humidity, 16).ToString();

                            //MSScriptControl.ScriptControl sc = new MSScriptControl.ScriptControlClass();
                            //sc.Language = "JavaScript";
                            Humidity = string.Format(payload.HumidityData.Expression, Humidity);
                            //Humidity = sc.Eval(Humidity).ToString();
                            //m_parser.SetExpr(Humidity);
                            Humidity = new DataTable().Compute(Humidity, "").ToString();
                            //Humidity = Microsoft.JScript.GlobalObject.eval(Humidity).ToString();
                            data.Humidity = Decimal.Parse(Humidity);
                        }
                        else
                        {
                            string Humidity = data.Payload.Substring(10, payload.HumidityData.Bits);

                            ////反转Payload数据, 原始数据 低位在前  高位在后
                            ////payload 数据格式 EB 02  EB是第一位 02是第二位 因此转成Hex进制时需要反转成0x02EB
                            //Humidity = Humidity.Substring(2, 2) + Humidity.Substring(0, 2);
                            ////公式算法
                            Humidity = Convert.ToInt64("0x" + Humidity, 16).ToString();

                            //MSScriptControl.ScriptControl sc = new MSScriptControl.ScriptControlClass();
                            //sc.Language = "JavaScript";
                            //Humidity = string.Format(payload.HumidityData.Expression, Humidity);
                            //Humidity = sc.Eval(Humidity).ToString();
                            //m_parser.SetExpr(Humidity);
                            //Humidity = new DataTable().Compute(Humidity, "").ToString();
                            //Humidity = Microsoft.JScript.GlobalObject.eval(Humidity).ToString();
                            data.Humidity = Decimal.Parse(Humidity) / 100 * 5;
                        }
                    }
                }

                if (payload.PressureSymbol != null)
                {
                    index = payload.PressureSymbol.StartBit;
                    if (index != -1)
                    {
                        string Pressure = data.Payload.Substring((index + payload.PressureSymbol.Bits), payload.PressureData.Bits);
                        Pressure = Pressure.Substring(2, 2) + Pressure.Substring(0, 2);
                        Pressure = Convert.ToInt64("0x" + Pressure, 16).ToString();
                        Pressure = string.Format(payload.PressureData.Expression, Pressure);
                        Pressure = new DataTable().Compute(Pressure, "").ToString();
                        data.Pressure = Decimal.Parse(Pressure);
                    }
                }
                if (payload.VoltageSymbol != null)
                {
                    index = payload.VoltageSymbol.StartBit;
                    if (index != -1)
                    {
                        string Voltage = data.Payload.Substring((index + payload.VoltageSymbol.Bits), payload.VoltageData.Bits);
                        Voltage = Voltage.Substring(0, 2);//
                        Voltage = Convert.ToInt64("0x" + Voltage, 16).ToString();
                        Voltage = string.Format(payload.VoltageData.Expression, Voltage);
                        Voltage = new DataTable().Compute(Voltage, "").ToString();
                        data.Voltage = Decimal.Parse(Voltage);
                    }
                }
                //处理高精度算法的温度
                if (payload.NodeType == NodeType.TemperaturePressureAccurate)
                {
                    //AA 75 16 62 00 00 36 56   9A 40 51   92 4C 00   93 9F 4A   9F E3 03  00 00 06 0E
                    //AA 75 16 62 00 00 36 56   9A 3E 51   92 4C 00   93 9F 4A   9F E3 03  00 00 06 0C
                    //                          9A 21 50   92 4C 00   93 9F 4A   9F F4 03
                    //StartBit="16" Length="18"
                    //StartBit="34" Length="6"
                    //温度部分
                    string Temperature = data.Payload.Substring(0, 18);
                    Decimal K = 3932160;

                    string x = data.Payload.Substring(2, 4);
                    x = x.Substring(2, 2) + x.Substring(0, 2);
                    //MSScriptControl.ScriptControl scX = new MSScriptControl.ScriptControlClass();
                    //scX.Language = "JavaScript";
                    x = string.Format("0x{0}", x);
                    x = Convert.ToInt64(x, 16).ToString();
                    //x = scX.Eval(x).ToString();
                    Decimal X = Decimal.Parse(x);

                    string x0 = data.Payload.Substring(8, 4);
                    x0 = x0.Substring(2, 2) + x0.Substring(0, 2);
                    //MSScriptControl.ScriptControl scX0 = new MSScriptControl.ScriptControlClass();
                    //scX0.Language = "JavaScript";
                    x0 = string.Format("0x{0}", x0);

                    x0 = Convert.ToInt64(x0, 16).ToString();
                    //x0 = scX0.Eval(x0).ToString();
                    Decimal X0 = Decimal.Parse(x0);

                    string x100 = data.Payload.Substring(14, 4);
                    x100 = x100.Substring(2, 2) + x100.Substring(0, 2);
                    //MSScriptControl.ScriptControl scX100 = new MSScriptControl.ScriptControlClass();
                    //scX100.Language = "JavaScript";
                    x100 = string.Format("0x{0}", x100);
                    //x100 = scX100.Eval(x100).ToString();
                    x100 = Convert.ToInt64(x100, 16).ToString();
                    Decimal X100 = Decimal.Parse(x100);

                    Decimal RX = X / (K - X);
                    Decimal R0 = X0 / (K - X0);
                    Decimal R100 = X100 / (K - X100);

                    Decimal t1 = 100 * (RX - R100);
                    Decimal t2 = 0.3851m * (R100 - R0);
                    data.Temperature = Math.Round(t1 / t2, 2);

                    //压力部分
                    string Pressure = data.Payload.Substring(18, 6);
                    Pressure = Pressure.Substring(4, 2) + Pressure.Substring(2, 2);
                    //MSScriptControl.ScriptControl sc = new MSScriptControl.ScriptControlClass();
                    //sc.Language = "JavaScript";
                    Pressure = string.Format("0x{0}", Pressure);
                    //Pressure = sc.Eval(Pressure).ToString();
                    Pressure = Convert.ToInt64(Pressure, 16).ToString();
                    data.Pressure = Decimal.Parse(Pressure);
                }
            }

            return data;
        }

        #endregion

    }
}
