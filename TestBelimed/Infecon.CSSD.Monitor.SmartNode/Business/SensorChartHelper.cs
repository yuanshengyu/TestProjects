using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Infecon.Common.Utility;
using Infecon.CSSD.Bll.Sensor;
using Infecon.CSSD.Business.Monitor;
using Infecon.CSSD.Entity.Sensor;
using Infecon.CSSD.Monitor.Common.SensorChart;
using log4net;
using Infecon.CSSD.Monitor.Common;

namespace Infecon.CSSD.Monitor.SmartNode.Business
{
    public class SensorChartHelper : ISensorChart
    {
        //日志记录
        protected static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private SensorDataHeadEntity mHeadEntity;

        private SensorEntity mSensor;
        private int mReadingInterval;

        // 同步最后的时间结点
        //private DateTime? mSyncLast;

        // 监控数据
        private DataTable mSensorData;

        private bool mIsCompressed = false;
        private SensorCompressedDataHelper mSensorCompressedData;

        private Dictionary<string, IList<SensorShowPropertyEntity>> mShowPropertyDic;

        #region ISensorChart 成员
        public SensorEntity Sensor
        {
            get { return mSensor; }
            set { mSensor = value; }
        }

        public int ReadingInterval
        {
            get { return mReadingInterval; }
            set { mReadingInterval = value; }
        }

        public string SensorStatusCaption 
        {
            get 
            {
                return String.Empty;
            }
        }

        public bool IsNew
        {
            get;
            set;
        }

        public ChartDTO GetChartData()
        {
            //mSyncLast = null;

            IsNew = true;

            mShowPropertyDic = GetShowPropertyData();

            // 刷新状态
            SensorHelper<object> helper = new SensorHelper<object>();
            mSensor = helper.SelectSingle<SensorEntity>(string.Format("SensorID = '{0}'", mSensor.SensorID.ToString().ToUpper()), string.Empty);
            
            SensorDataHeadBll bllHead = new SensorDataHeadBll();
            SensorDataHeadEntity eHead = bllHead.GetNewestSensorDataHeadBySensorID(Sensor.SensorID.ToString().ToUpper());
            if (eHead == null)
            {
                ChartDTO chartData = new ChartDTO();
                chartData.IsAxisXCustomLabels = false;
                chartData.ChartTitle = GetChartTitle();
                return chartData;
            }
            else
            {
                if (eHead.IsCompressed != null && eHead.IsCompressed.HasValue && eHead.IsCompressed.Value)
                {
                    ChartDTO chartData = new ChartDTO();
                    chartData.IsAxisXCustomLabels = false;
                    chartData.ChartTitle = GetChartTitle();
                    return chartData;
                }

                return GetChartDataByHead(eHead);
            }
        }

        public ChartDTO GetChartData(string sdhID)
        {
            //mSyncLast = null;

            IsNew = true;

            mShowPropertyDic = GetShowPropertyData();

            // 刷新状态
            SensorHelper<object> helper = new SensorHelper<object>();
            mSensor = helper.SelectSingle<SensorEntity>(string.Format("SensorID = '{0}'", mSensor.SensorID.ToString().ToUpper()), string.Empty);
            
            SensorDataHeadBll bllHead = new SensorDataHeadBll();
            SensorDataHeadEntity eHead = bllHead.GetEntity(new Guid(sdhID));
            if (eHead == null)
            {
                return null;
            }
            else
            {
                if (eHead.IsCompressed != null && eHead.IsCompressed.HasValue && eHead.IsCompressed.Value)
                {
                    // 已压缩
                    mIsCompressed = true;
                    mSensorCompressedData = new SensorCompressedDataHelper(eHead);
                }
                
                return GetChartDataByHead(eHead);
            }
        }

        public ChartDTO GetChartDataInc(DateTime? lastReceivedDate, bool canChangeHead)
        {
            //mSyncLast = null;

            IsNew = false;

            // 刷新状态
            SensorHelper<object> helper = new SensorHelper<object>();
            mSensor = helper.SelectSingle<SensorEntity>(string.Format("SensorID = '{0}'", mSensor.SensorID.ToString().ToUpper()), string.Empty);

            SensorDataHeadBll bllHead = new SensorDataHeadBll();

            SensorDataHeadEntity eHead = bllHead.GetNewestSensorDataHeadBySensorID(Sensor.SensorID.ToString().ToUpper());
            if (eHead == null)
            {
                return null;
            }
            else
            {
                if (eHead.IsCompressed != null && eHead.IsCompressed.HasValue && eHead.IsCompressed.Value)
                {
                    ChartDTO chartData = new ChartDTO();
                    chartData.IsAxisXCustomLabels = false;
                    chartData.ChartTitle = GetChartTitle();
                    chartData.IsReset = true;
                    return chartData;
                }

                if (mHeadEntity != null && eHead.SensorDataHeadID == mHeadEntity.SensorDataHeadID && lastReceivedDate != null && lastReceivedDate.HasValue)
                {
                    return GetChartDataInc(eHead, lastReceivedDate.Value);
                }
                else
                {
                    if (canChangeHead)
                    {
                        mSensorData = null;
                        ChartDTO dtoChart = GetChartDataByHead(eHead);
                        dtoChart.IsReset = true;
                        IsNew = true;
                        return dtoChart;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        public DataTable GetSensorData()
        {
            return mSensorData;
        }

        public IList<KeyValuePair<string, string>> GetSensorHead()
        {
            if (mSensor == null || mHeadEntity == null)
            {
                return null;
            }
            else
            {
                IList<KeyValuePair<string, string>> lstHead = new List<KeyValuePair<string, string>>();

                lstHead.Add(new KeyValuePair<string, string>("设备名称", mSensor.SensorKey));
                lstHead.Add(new KeyValuePair<string, string>(Common.Consts.C_BLANK_PREFIX, string.Empty));
                lstHead.Add(new KeyValuePair<string, string>(Common.Consts.C_RETURN_PREFIX, string.Empty));
                if (mHeadEntity.BeginDate != null && mHeadEntity.BeginDate.HasValue)
                {
                    lstHead.Add(new KeyValuePair<string, string>("开始时间", mHeadEntity.BeginDate.Value.ToString("yyyy-MM-dd HH:mm:ss")));
                }
                else
                {
                    lstHead.Add(new KeyValuePair<string, string>("开始时间", string.Empty));
                }
                if (mHeadEntity.EndDate != null && mHeadEntity.EndDate.HasValue)
                {
                    lstHead.Add(new KeyValuePair<string, string>("结束时间", mHeadEntity.EndDate.Value.ToString("yyyy-MM-dd HH:mm:ss")));
                }
                else
                {
                    lstHead.Add(new KeyValuePair<string, string>("结束时间", string.Empty));
                }

                return lstHead;
            }
        }

        public Dictionary<string, IList<SensorShowPropertyEntity>> GetShowProperty()
        {
            return mShowPropertyDic;
        }

        #endregion

        private Dictionary<string, IList<SensorShowPropertyEntity>> GetShowPropertyData()
        {
            Dictionary<string, IList<SensorShowPropertyEntity>> dicShowProperty = new Dictionary<string, IList<SensorShowPropertyEntity>>();

            GetSeriesShowProperty(dicShowProperty, Properties.Resource.TemperatureSeries);
            GetSeriesShowProperty(dicShowProperty, Properties.Resource.PressureSeries);
            GetSeriesShowProperty(dicShowProperty, Properties.Resource.HumiditySeries);
            GetSeriesShowProperty(dicShowProperty, Properties.Resource.VoltageSeries);

            return dicShowProperty;
        }

        private void GetSeriesShowProperty(Dictionary<string, IList<SensorShowPropertyEntity>> dicShowProperty, string showName)
        {
            SensorHelper<object> helper = new SensorHelper<object>();

            string strWhere = string.Format("SensorGroup = '{0}' AND SensorType = '{1}' AND ShowName = '{2}'", mSensor.SensorGroup, mSensor.SensorType, showName);
            IList<SensorShowPropertyEntity> lstShowProperty = helper.SelectList<SensorShowPropertyEntity>(strWhere, string.Empty);

            if (lstShowProperty != null && lstShowProperty.Count > 0)
            {
                dicShowProperty.Add(showName, lstShowProperty);
            }
        }

        private ChartDTO GetChartDataByHead(SensorDataHeadEntity eHead)
        {
            mHeadEntity = eHead;
            if (mHeadEntity == null)
            {
                return null;
            }

            IList<SensorDataLineEntity> lstLine = null;
            SensorHelper<object> helper = new SensorHelper<object>();
            if (mIsCompressed)
            {
                lstLine = mSensorCompressedData.GetSensorDataLine();
            }
            else
            {
                string strWhere = string.Format("SensorDataHeadID = '{0}'", eHead.SensorDataHeadID.ToString().ToUpper());
                lstLine = helper.SelectList<SensorDataLineEntity>(strWhere, "ReceivedDate");
            }

            SeriesDTO sTemperature = null;
            SeriesDTO sPressure = null;
            SeriesDTO sHumidity = null;
            SeriesDTO sVoltage = null;

            GetSeriesDto(lstLine, ref sTemperature, ref sPressure, ref sHumidity, ref sVoltage);

            MakeSensorData(lstLine);

            ChartDTO chartData = new ChartDTO();
            chartData.IsAxisXCustomLabels = false;

            if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Temperature))
                || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperatureHumidity))
                || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperaturePressure))
                || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperaturePressureAccurate)))
            {
                // 温度
                AxesDTO axesTemperature = new AxesDTO(Properties.Resource.TemperatureSeries);
                axesTemperature.AxisYProperty.Add(AxesDTO.AxisProperty.EndText.ToString(), Properties.Resource.TemperatureUnit);
                Dictionary<string, SeriesDTO> dicSeriesTemperature = new Dictionary<string, SeriesDTO>();
                dicSeriesTemperature.Add(Properties.Resource.TemperatureSeries, sTemperature);

                chartData.AxesDictionary.Add(Properties.Resource.TemperatureSeries, axesTemperature);
                chartData.SeriesDictionary.Add(Properties.Resource.TemperatureSeries, dicSeriesTemperature);
            }

            if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Humidity))
                || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperatureHumidity)))
            {
                // 湿度
                AxesDTO axesHumidity = new AxesDTO(Properties.Resource.HumiditySeries);
                axesHumidity.AxisYProperty.Add(AxesDTO.AxisProperty.EndText.ToString(), Properties.Resource.HumidityUnit);
                Dictionary<string, SeriesDTO> dicSeriesHumidity = new Dictionary<string, SeriesDTO>();
                dicSeriesHumidity.Add(Properties.Resource.HumiditySeries, sHumidity);

                chartData.AxesDictionary.Add(Properties.Resource.HumiditySeries, axesHumidity);
                chartData.SeriesDictionary.Add(Properties.Resource.HumiditySeries, dicSeriesHumidity);
            }

            if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Pressure))
                || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperaturePressure))
                || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperaturePressureAccurate)))
            {
                // 气压
                AxesDTO axesPressure = new AxesDTO(Properties.Resource.PressureSeries);
                axesPressure.AxisYProperty.Add(AxesDTO.AxisProperty.EndText.ToString(), Properties.Resource.PressureUnit);
                Dictionary<string, SeriesDTO> dicSeriesPressure = new Dictionary<string, SeriesDTO>();
                dicSeriesPressure.Add(Properties.Resource.PressureSeries, sPressure);

                chartData.AxesDictionary.Add(Properties.Resource.PressureSeries, axesPressure);
                chartData.SeriesDictionary.Add(Properties.Resource.PressureSeries, dicSeriesPressure);
            }

            chartData.ChartTitle = GetChartTitle();
            chartData.RealtimeInfo = GetRealtimeInfo(lstLine);

            chartData.Comment = GetComment(chartData.SeriesDictionary);

            chartData.ErrorData = null;

            return chartData;
        }

        private ChartDTO GetChartDataInc(SensorDataHeadEntity eHead, DateTime lastReceivedDate)
        {
            IList<SensorDataLineEntity> lstLine = null;
            SensorHelper<object> helper = new SensorHelper<object>();
            if (mIsCompressed)
            {
                lstLine = mSensorCompressedData.GetSensorDataLine();
            }
            else
            {
                string strWhere = string.Empty;
                if (CommonValue.DatabaseType == CommonValue.DatabaseFlag.Oracle)
                {
                    strWhere = string.Format("SensorDataHeadID = '{0}' AND to_char(ReceivedDate, 'YYYY-MM-DD HH24:MI:SS.FF3') > '{1}'", eHead.SensorDataHeadID.ToString().ToUpper(), lastReceivedDate.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                }
                else
                {
                    strWhere = string.Format("SensorDataHeadID = '{0}' AND ReceivedDate > '{1}'", eHead.SensorDataHeadID.ToString().ToUpper(), lastReceivedDate.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                }
                lstLine = helper.SelectList<SensorDataLineEntity>(strWhere, "ReceivedDate");
            }

            SeriesDTO sTemperature = null;
            SeriesDTO sPressure = null;
            SeriesDTO sHumidity = null;
            SeriesDTO sVoltage = null;

            GetSeriesDto(lstLine, ref sTemperature, ref sPressure, ref sHumidity, ref sVoltage);

            MakeSensorData(lstLine);

            ChartDTO chartData = new ChartDTO();
            chartData.IsAxisXCustomLabels = false;

            if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Temperature))
                || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperatureHumidity))
                || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperaturePressure))
                || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperaturePressureAccurate)))
            {
                // 温度
                Dictionary<string, SeriesDTO> dicSeriesTemperature = new Dictionary<string, SeriesDTO>();
                dicSeriesTemperature.Add(Properties.Resource.TemperatureSeries, sTemperature);
                chartData.SeriesDictionary.Add(Properties.Resource.TemperatureSeries, dicSeriesTemperature);
            }

            if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Humidity))
                || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperatureHumidity)))
            {
                // 湿度
                Dictionary<string, SeriesDTO> dicSeriesHumidity = new Dictionary<string, SeriesDTO>();
                dicSeriesHumidity.Add(Properties.Resource.HumiditySeries, sHumidity);
                chartData.SeriesDictionary.Add(Properties.Resource.HumiditySeries, dicSeriesHumidity);
            }

            if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Pressure))
                || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperaturePressure))
                || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperaturePressureAccurate)))
            {
                // 气压
                Dictionary<string, SeriesDTO> dicSeriesPressure = new Dictionary<string, SeriesDTO>();
                dicSeriesPressure.Add(Properties.Resource.PressureSeries, sPressure);
                chartData.SeriesDictionary.Add(Properties.Resource.PressureSeries, dicSeriesPressure);
            }

            chartData.ChartTitle = GetChartTitle();
            chartData.RealtimeInfo = GetRealtimeInfo(lstLine);

            chartData.Comment = GetComment(chartData.SeriesDictionary);

            chartData.ErrorData = null;

            return chartData;
        }
        
        private void GetSeriesDto(IList<SensorDataLineEntity> lstLine, ref SeriesDTO sTemperature, ref SeriesDTO sPressure, ref SeriesDTO sHumidity, ref SeriesDTO sVoltage)
        {
            if (lstLine != null && lstLine.Count > 0)
            {
                IList<SeriesPointDTO> lstTemperaturePoints = new List<SeriesPointDTO>();
                IList<SeriesPointDTO> lstPressurePoints = new List<SeriesPointDTO>();
                IList<SeriesPointDTO> lstHumidityPoints = new List<SeriesPointDTO>();
                IList<SeriesPointDTO> lstVoltagePoints = new List<SeriesPointDTO>();

                int spCount = 0;
                DateTime? curSensorDate = null;
                DateTime? preSensorDate = null;
                double dif = 0;

                foreach (SensorDataLineEntity sdle in lstLine)
                {
                    curSensorDate = ParseHelper.ParseToDateTime(sdle.ReceivedDate.Value);

                    // 间隔
                    spCount++;
                    if (spCount > 1 && spCount < lstLine.Count)
                    {
                        dif = curSensorDate.Value.Subtract(preSensorDate.Value).TotalMilliseconds;

                        if (dif < mReadingInterval)
                        {
                            continue;
                        }
                    }

                    MakeSeriesPoint(lstTemperaturePoints, curSensorDate, sdle.ValueNumber1);    //Temperature
                    MakeSeriesPoint(lstPressurePoints, curSensorDate, sdle.ValueNumber2);       //Pressure
                    MakeSeriesPoint(lstHumidityPoints, curSensorDate, sdle.ValueNumber3);       //Humidity
                    MakeSeriesPoint(lstVoltagePoints, curSensorDate, sdle.ValueNumber4);        //Voltage

                    preSensorDate = curSensorDate;
                }

                sTemperature = MakeSeries(lstTemperaturePoints, Properties.Resource.TemperatureSeries);
                sPressure = MakeSeries(lstPressurePoints, Properties.Resource.PressureSeries);
                sHumidity = MakeSeries(lstHumidityPoints, Properties.Resource.HumiditySeries);
                sVoltage = MakeSeries(lstVoltagePoints, Properties.Resource.VoltageSeries);
            }
        }

        private string GetRealtimeInfo(IList<SensorDataLineEntity> lstLine)
        {
            if (lstLine != null && lstLine.Count > 0)
            {
                SensorDataLineEntity eLine = lstLine[lstLine.Count - 1];
                StringBuilder sbRealtimeInfo = new StringBuilder();


                if (eLine.ValueNumber1 != null && eLine.ValueNumber1.HasValue)
                {
                    sbRealtimeInfo.AppendLine(string.Format("{0}: {1}", Properties.Resource.TemperatureSeries, eLine.ValueNumber1.Value.ToString("0.00")));
                }
                if (eLine.ValueNumber2 != null && eLine.ValueNumber2.HasValue)
                {
                    sbRealtimeInfo.AppendLine(string.Format("{0}: {1}", Properties.Resource.PressureSeries, eLine.ValueNumber2.Value.ToString("0.00")));
                }
                if (eLine.ValueNumber3 != null && eLine.ValueNumber3.HasValue)
                {
                    sbRealtimeInfo.AppendLine(string.Format("{0}: {1}", Properties.Resource.HumiditySeries, eLine.ValueNumber3.Value.ToString("0.00")));
                }
                if (eLine.ValueNumber4 != null && eLine.ValueNumber4.HasValue)
                {
                    sbRealtimeInfo.AppendLine(string.Format("{0}: {1}", Properties.Resource.VoltageSeries, eLine.ValueNumber4.Value.ToString("0.00")));
                }

                return sbRealtimeInfo.ToString();
            }

            return string.Empty;
        }

        private string GetChartTitle()
        {
            return mSensor.SensorName;
        }

        private static string GetComment(Dictionary<string, Dictionary<string, SeriesDTO>> dicSeries)
        {
            if (dicSeries == null || dicSeries.Count == 0)
            {
                return string.Empty;
            }

            string strComment = string.Empty;
            foreach (KeyValuePair<string, Dictionary<string, SeriesDTO>> kvDic in dicSeries)
            {
                if (kvDic.Value != null)
                {
                    foreach (KeyValuePair<string, SeriesDTO> kv in kvDic.Value)
                    {
                        SeriesDTO dtoSeries = kv.Value;
                        if (dtoSeries != null && dtoSeries.Points != null && dtoSeries.Points.Count > 0)
                        {
                            DateTime dtEnd = Convert.ToDateTime(dtoSeries.Points[dtoSeries.Points.Count - 1].Argument);
                            strComment = string.Format("更新时间：{0}", dtEnd.ToString("HH:mm:ss"));
                            break;
                        }
                    }
                }
            }

            return strComment;
        }

        private void MakeSeriesPoint(IList<SeriesPointDTO> lstPoints, DateTime? curSensorDate, decimal? valueNumber)
        {
            if (valueNumber != null && valueNumber.HasValue)
            {
                SeriesPointDTO sp = new SeriesPointDTO();
                sp.Argument = curSensorDate.Value.ToString("yyyy-MM-dd HH:mm:ss.fff");
                sp.Values = new double[] { Convert.ToDouble(valueNumber.Value) };
                sp.ReceivedDate = curSensorDate;
                lstPoints.Add(sp);
            }
        }

        private SeriesDTO MakeSeries(IList<SeriesPointDTO> lstPoints, string seriesName)
        {
            if (lstPoints.Count > 0)
            {
                SeriesDTO s = new SeriesDTO();
                s.SeriesTitle = seriesName;
                s.Points = lstPoints;

                return s;
            }
            else
            {
                return null;
            }
        }

        private void MakeSensorData(IList<SensorDataLineEntity> lstLine)
        {
            if (mSensorData == null)
            {
                mSensorData = new DataTable("SensorData");
                mSensorData.Columns.Add("序号", typeof(int));
                mSensorData.Columns.Add("接收时间", typeof(string));

                if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Temperature))
                || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperatureHumidity))
                || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperaturePressure))
                || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperaturePressureAccurate)))
                {
                    mSensorData.Columns.Add(Properties.Resource.TemperatureSeries, typeof(decimal));
                }
                if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Humidity))
                || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperatureHumidity)))
                {
                    // 湿度
                    mSensorData.Columns.Add(Properties.Resource.HumiditySeries, typeof(decimal));
                }
                if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Pressure))
                || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperaturePressure))
                || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperaturePressureAccurate)))
                {
                    // 气压
                    mSensorData.Columns.Add(Properties.Resource.PressureSeries, typeof(decimal));
                }
                // 电压
                mSensorData.Columns.Add(Properties.Resource.VoltageSeries, typeof(decimal));
            }

            int index = mSensorData.Rows.Count;
            mSensorData.BeginLoadData();
            for (int i = 0; i < lstLine.Count; i++)
            {

                DataRow row = mSensorData.NewRow();
                row["序号"] = ++index;

                DateTime? dtReceive = lstLine[i].ReceivedDate;
                row["接收时间"] = dtReceive.Value.ToString("yyyy-MM-dd HH:mm:ss.fff");

                if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Temperature))
                || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperatureHumidity))
                || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperaturePressure))
                || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperaturePressureAccurate)))
                {
                    row[Properties.Resource.TemperatureSeries] = lstLine[i].ValueNumber1;
                }
                if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Humidity))
                || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperatureHumidity)))
                {
                    // 湿度
                    row[Properties.Resource.HumiditySeries] = lstLine[i].ValueNumber3;
                }
                if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Pressure))
                || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperaturePressure))
                || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperaturePressureAccurate)))
                {
                    // 气压
                    row[Properties.Resource.PressureSeries] = lstLine[i].ValueNumber2;
                }
                // 电压
                row[Properties.Resource.VoltageSeries] = lstLine[i].ValueNumber4;

                mSensorData.Rows.Add(row);
            }
            mSensorData.EndLoadData();

        }


    }
}
