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

namespace Infecon.CSSD.Monitor.Belimed.Business
{
    public class SensorChartHelper : ISensorChart
    {
        //日志记录
        protected static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private SensorDataHeadEntity mHeadEntity;

        private SensorEntity mSensor;
        private int mReadingInterval;

        // 同步最后的时间结点
        private DateTime? mSyncLast;

        private Dictionary<string, DataRow[]> mDataDic;
        private DataTable mSensorData;

        //private IList<SensorDataLineEntity> mSectionList;

        private DataTable mProgramData;

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
                return Utility.GetStatusCaption(mSensor.PositionMark); 
            }
        }

        public bool IsNew
        {
            get;
            set;
        }

        public ChartDTO GetChartData()
        {
            mSyncLast = null;
            mDataDic = new Dictionary<string, DataRow[]>();

            IsNew = true;

            mProgramData = null;

            mShowPropertyDic = GetShowPropertyData();

            // 刷新状态
            SensorHelper<object> helper = new SensorHelper<object>();
            mSensor = helper.SelectSingle<SensorEntity>(string.Format("SensorID = '{0}'", mSensor.SensorID.ToString().ToUpper()), string.Empty);
            
            SensorDataHeadBll bllHead = new SensorDataHeadBll();
            SensorDataHeadEntity eHead = bllHead.GetNewestSensorDataHeadBySensorID(Sensor.SensorID.ToString().ToUpper());
            if (eHead == null)
            {
                ChartDTO chartData = new ChartDTO();
                chartData.ChartTitle = GetChartTitle();
                return chartData;
            }
            else
            {
                if (eHead.IsCompressed != null && eHead.IsCompressed.HasValue && eHead.IsCompressed.Value)
                {
                    ChartDTO chartData = new ChartDTO();
                    chartData.ChartTitle = GetChartTitle();
                    return chartData;
                }

                //SensorDataLineBll bllLine = new SensorDataLineBll();
                //mSectionList = bllLine.GetSensorDataLineByDataType(eHead.SensorDataHeadID.ToString(), Utility.GetEnumValue(DataType.Body_SectionTitle));
                mProgramData = GetProgramData(eHead);

                return GetChartDataByHead(eHead);
            }
        }

        public ChartDTO GetChartData(string sdhID)
        {
            mSyncLast = null;
            mDataDic = new Dictionary<string, DataRow[]>();

            IsNew = true;

            mProgramData = null;

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
                
                //SensorDataLineBll bllLine = new SensorDataLineBll();
                //mSectionList = bllLine.GetSensorDataLineByDataType(eHead.SensorDataHeadID.ToString(), Utility.GetEnumValue(DataType.Body_SectionTitle));
                mProgramData = GetProgramData(eHead);

                return GetChartDataByHead(eHead);
            }
        }

        public ChartDTO GetChartDataInc(DateTime? lastReceivedDate, bool canChangeHead)
        {
            mSyncLast = null;
            mDataDic = new Dictionary<string, DataRow[]>();

            IsNew = false;

            mProgramData = null;

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
                if (mHeadEntity != null && eHead.SensorDataHeadID != mHeadEntity.SensorDataHeadID && !canChangeHead)
                {
                    return null;
                }

                if (eHead.IsCompressed != null && eHead.IsCompressed.HasValue && eHead.IsCompressed.Value)
                {
                    if (canChangeHead)
                    {
                        ChartDTO chartData = new ChartDTO();
                        chartData.ChartTitle = GetChartTitle();
                        chartData.IsReset = true;
                        return chartData;
                    }
                    else
                    {
                        return null;
                    }
                }

                //SensorDataLineBll bllLine = new SensorDataLineBll();
                //mSectionList = bllLine.GetSensorDataLineByDataType(eHead.SensorDataHeadID.ToString(), Utility.GetEnumValue(DataType.Body_SectionTitle));

                if (mHeadEntity != null && eHead.SensorDataHeadID == mHeadEntity.SensorDataHeadID && lastReceivedDate != null && lastReceivedDate.HasValue)
                {
                    mProgramData = GetProgramDataInc(eHead, lastReceivedDate.Value);

                    if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Sterilizer)))
                    {
                        // 灭菌
                        return GetSterilizerChartDataInc(eHead, lastReceivedDate.Value);
                    }
                    else if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Washer)))
                    {
                        // 清洗
                        return GetWasherChartDataInc(eHead, lastReceivedDate.Value);
                    }
                    else
                    {
                        logger.WarnFormat("未处理的设备类型（[传感器：{0}，设备类型：{1}]）", Sensor.SensorKey, Sensor.SensorType);

                        return null;
                    }
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

        private Dictionary<string, IList<SensorShowPropertyEntity>> GetShowPropertyData()
        {
            Dictionary<string, IList<SensorShowPropertyEntity>> dicShowProperty = new Dictionary<string, IList<SensorShowPropertyEntity>>();

            // WasherSeries1 ~ WasherSeries5
            GetSeriesShowProperty(dicShowProperty, Properties.Resource.WasherSeries1);
            GetSeriesShowProperty(dicShowProperty, Properties.Resource.WasherSeries2);
            GetSeriesShowProperty(dicShowProperty, Properties.Resource.WasherSeries3);
            GetSeriesShowProperty(dicShowProperty, Properties.Resource.WasherSeries4);
            GetSeriesShowProperty(dicShowProperty, Properties.Resource.WasherSeries5);

            // SterilizerSeries1 ~ SterilizerSeries6
            GetSeriesShowProperty(dicShowProperty, Properties.Resource.SterilizerSeries1);
            GetSeriesShowProperty(dicShowProperty, Properties.Resource.SterilizerSeries2);
            GetSeriesShowProperty(dicShowProperty, Properties.Resource.SterilizerSeries3);
            GetSeriesShowProperty(dicShowProperty, Properties.Resource.SterilizerSeries4);
            GetSeriesShowProperty(dicShowProperty, Properties.Resource.SterilizerSeries5);
            GetSeriesShowProperty(dicShowProperty, Properties.Resource.SterilizerSeries6);

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


        private void MergeSterilizerSensorData()
        {
            if (mSensorData == null)
            {
                mSensorData = new DataTable("SensorData");
                mSensorData.Columns.Add("序号", typeof(int));
                mSensorData.Columns.Add("接收时间", typeof(string));
                mSensorData.Columns.Add("传感器时间", typeof(string));
                mSensorData.Columns.Add("已用时间(毫秒)", typeof(long));

                mSensorData.Columns.Add(Properties.Resource.ProgramNo, typeof(string));
                mSensorData.Columns.Add(Properties.Resource.ProgramName, typeof(string));
                mSensorData.Columns.Add(Properties.Resource.ProgramPhase, typeof(string));

                mSensorData.Columns.Add(Properties.Resource.SterilizerSeries1, typeof(decimal));
                mSensorData.Columns.Add(Properties.Resource.SterilizerSeries2, typeof(decimal));
                mSensorData.Columns.Add(Properties.Resource.SterilizerSeries3, typeof(decimal));
                mSensorData.Columns.Add(Properties.Resource.SterilizerSeries4, typeof(decimal));
                mSensorData.Columns.Add(Properties.Resource.SterilizerSeries5, typeof(decimal));
                mSensorData.Columns.Add(Properties.Resource.SterilizerSeries6, typeof(decimal));
            }

            DataRow[] drS1 = mDataDic[Properties.Resource.SterilizerSeries1];
            DataRow[] drS2 = mDataDic[Properties.Resource.SterilizerSeries2];
            DataRow[] drS3 = mDataDic[Properties.Resource.SterilizerSeries3];
            DataRow[] drS4 = mDataDic[Properties.Resource.SterilizerSeries4];
            DataRow[] drS5 = mDataDic[Properties.Resource.SterilizerSeries5];
            DataRow[] drS6 = mDataDic[Properties.Resource.SterilizerSeries6];

            if (drS1 == null)
            {
                return;
            }

            int index = mSensorData.Rows.Count;

            mSensorData.BeginLoadData();
            for (int i = 0; i < mDataDic[Properties.Resource.SterilizerSeries1].Length; i++)
            {

                DataRow row = mSensorData.NewRow();
                row["序号"] = ++index;

                DateTime? dtReceive = ParseHelper.ParseToDateTime(drS1[i]["ReceivedDate"]);
                row["接收时间"] = dtReceive.Value.ToString("yyyy-MM-dd HH:mm:ss.fff");

                DateTime? dtSensorDate = ParseHelper.ParseToDateTime(drS1[i]["SensorDate"]);
                row["传感器时间"] = dtReceive.Value.ToString("yyyy-MM-dd HH:mm:ss.fff");

                long sts = 0;
                if (dtSensorDate != null && dtSensorDate.HasValue)
                {
                    sts = Convert.ToInt64(dtSensorDate.Value.Subtract(mHeadEntity.BeginDate.Value).TotalMilliseconds);
                }
                row["已用时间(毫秒)"] = sts;

                if (mProgramData != null)
                {
                    row[Properties.Resource.ProgramNo] = mProgramData.Rows[i][Properties.Resource.ProgramNo];
                    row[Properties.Resource.ProgramName] = mProgramData.Rows[i][Properties.Resource.ProgramName];
                    row[Properties.Resource.ProgramPhase] = mProgramData.Rows[i][Properties.Resource.ProgramPhase];
                }

                row[Properties.Resource.SterilizerSeries1] = drS1[i]["ContentNumber"];
                row[Properties.Resource.SterilizerSeries2] = drS2[i]["ContentNumber"];
                row[Properties.Resource.SterilizerSeries3] = drS3[i]["ContentNumber"];
                row[Properties.Resource.SterilizerSeries4] = drS4[i]["ContentNumber"];
                row[Properties.Resource.SterilizerSeries5] = drS5[i]["ContentNumber"];
                row[Properties.Resource.SterilizerSeries6] = drS6[i]["ContentNumber"];

                mSensorData.Rows.Add(row);
            }
            mSensorData.EndLoadData();
        }

        private void MergeWasherSensorData()
        {
            if (mSensorData == null)
            {
                mSensorData = new DataTable("SensorData");
                mSensorData.Columns.Add("序号", typeof(int));
                mSensorData.Columns.Add("接收时间", typeof(string));
                mSensorData.Columns.Add("传感器时间", typeof(string));
                mSensorData.Columns.Add("已用时间(毫秒)", typeof(long));
                
                mSensorData.Columns.Add(Properties.Resource.ProgramNo, typeof(string));
                mSensorData.Columns.Add(Properties.Resource.ProgramName, typeof(string));
                mSensorData.Columns.Add(Properties.Resource.ProgramPhase, typeof(string));

                mSensorData.Columns.Add(Properties.Resource.WasherSeries1, typeof(decimal));
                mSensorData.Columns.Add(Properties.Resource.WasherSeries2, typeof(decimal));
                mSensorData.Columns.Add(Properties.Resource.WasherSeries3, typeof(decimal));
                mSensorData.Columns.Add(Properties.Resource.WasherSeries4, typeof(decimal));
                mSensorData.Columns.Add(Properties.Resource.WasherSeries5, typeof(decimal));
            }

            DataRow[] drS1 = mDataDic[Properties.Resource.WasherSeries1];
            DataRow[] drS2 = mDataDic[Properties.Resource.WasherSeries2];
            DataRow[] drS3 = mDataDic[Properties.Resource.WasherSeries3];
            DataRow[] drS4 = mDataDic[Properties.Resource.WasherSeries4];
            DataRow[] drS5 = mDataDic[Properties.Resource.WasherSeries5];

            if (drS1 == null)
            {
                return;
            }

            int index = mSensorData.Rows.Count;

            mSensorData.BeginLoadData();
            for (int i = 0; i < drS1.Length; i++)
            {

                DataRow row = mSensorData.NewRow();
                row["序号"] = ++index;

                DateTime? dtReceive = ParseHelper.ParseToDateTime(drS1[i]["ReceivedDate"]);
                row["接收时间"] = dtReceive.Value.ToString("yyyy-MM-dd HH:mm:ss.fff");

                DateTime? dtSensorDate = ParseHelper.ParseToDateTime(drS1[i]["SensorDate"]);
                row["传感器时间"] = dtReceive.Value.ToString("yyyy-MM-dd HH:mm:ss.fff");

                long sts = 0;
                if (dtSensorDate != null && dtSensorDate.HasValue)
                {
                    sts = Convert.ToInt64(dtSensorDate.Value.Subtract(mHeadEntity.BeginDate.Value).TotalMilliseconds);
                }
                row["已用时间(毫秒)"] = sts;

                if (mProgramData != null)
                {
                    row[Properties.Resource.ProgramNo] = mProgramData.Rows[i][Properties.Resource.ProgramNo];
                    row[Properties.Resource.ProgramName] = mProgramData.Rows[i][Properties.Resource.ProgramName];
                    row[Properties.Resource.ProgramPhase] = mProgramData.Rows[i][Properties.Resource.ProgramPhase];
                }

                row[Properties.Resource.WasherSeries1] = drS1[i]["ContentNumber"];
                row[Properties.Resource.WasherSeries2] = drS2[i]["ContentNumber"];
                row[Properties.Resource.WasherSeries3] = drS3[i]["ContentNumber"];
                row[Properties.Resource.WasherSeries4] = drS4[i]["ContentNumber"];
                row[Properties.Resource.WasherSeries5] = drS5[i]["ContentNumber"];

                mSensorData.Rows.Add(row);
            }
            mSensorData.EndLoadData();
        }

        #endregion

        private ChartDTO GetChartDataByHead(SensorDataHeadEntity eHead)
        {
            mHeadEntity = eHead;
            if (mHeadEntity == null)
            {
                return null;
            }

            if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Sterilizer)))
            {
                // 灭菌
                return GetSterilizerChartData(mHeadEntity);
            }
            else if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Washer)))
            {
                // 清洗
                return GetWasherChartData(mHeadEntity);
            }
            else
            {
                logger.WarnFormat("未处理的设备类型（[传感器：{0}，设备类型：{1}]）", Sensor.SensorKey, Sensor.SensorType);

                return null;
            }

            //return null;
        }

        private ChartDTO GetWasherChartData(SensorDataHeadEntity eHead)
        {
            SeriesDTO s1 = null;
            SeriesDTO s2 = null;
            SeriesDTO s3 = null;
            SeriesDTO s4 = null;
            SeriesDTO s5 = null;
            if (mIsCompressed)
            {
                s1 = GetCompressedSeries(eHead, SystemData.MeasurementSensor1, Properties.Resource.WasherSeries1);
                s2 = GetCompressedSeries(eHead, SystemData.MeasurementSensor2, Properties.Resource.WasherSeries2);
                s3 = GetCompressedSeries(eHead, SystemData.MeasurementSensor3, Properties.Resource.WasherSeries3);
                s4 = GetCompressedSeries(eHead, SystemData.MeasurementSensor4, Properties.Resource.WasherSeries4);
                s5 = GetCompressedSeries(eHead, SystemData.MeasurementSensor5, Properties.Resource.WasherSeries5);
            }
            else
            {
                s1 = GetSeries(eHead, SystemData.MeasurementSensor1, Properties.Resource.WasherSeries1);
                s2 = GetSeries(eHead, SystemData.MeasurementSensor2, Properties.Resource.WasherSeries2);
                s3 = GetSeries(eHead, SystemData.MeasurementSensor3, Properties.Resource.WasherSeries3);
                s4 = GetSeries(eHead, SystemData.MeasurementSensor4, Properties.Resource.WasherSeries4);
                s5 = GetSeries(eHead, SystemData.MeasurementSensor5, Properties.Resource.WasherSeries5);
            }

            MergeWasherSensorData();

            AxesDTO axesTemp = new AxesDTO(AxesName.Temperature.ToString());
            axesTemp.AxisYProperty.Add(AxesDTO.AxisProperty.EndText.ToString(), Properties.Resource.TempUnit);
            //axesTemp.SeriesList.Add(sTempIndepCham);

            AxesDTO axesAo = new AxesDTO(AxesName.AoValue.ToString());
            axesAo.AxisYProperty.Add(AxesDTO.AxisProperty.MinValueSerializable.ToString(), Properties.Resource.A0MinValue);
            axesAo.AxisYProperty.Add(AxesDTO.AxisProperty.MaxValueSerializable.ToString(), Properties.Resource.A0MaxValue);

            Dictionary<string, SeriesDTO> dicSeriesTemp = new Dictionary<string, SeriesDTO>();
            dicSeriesTemp.Add(Properties.Resource.WasherSeries2, s2);
            Dictionary<string, SeriesDTO> dicSeriesAo = new Dictionary<string, SeriesDTO>();
            dicSeriesAo.Add(Properties.Resource.WasherSeries5, s5);
            dicSeriesAo.Add(Properties.Resource.WasherSeries1, s1);
            dicSeriesAo.Add(Properties.Resource.WasherSeries3, s3);
            dicSeriesAo.Add(Properties.Resource.WasherSeries4, s4);

            ChartDTO chartData = new ChartDTO();

            string temp = string.Empty;
            if (s2 != null && s2.Points.Count > 0)
            {
                temp = s2.Points[s2.Points.Count - 1].Values[0].ToString("#.0");
            }
            string a0 = string.Empty;
            if (s5 != null && s5.Points.Count > 0)
            {
                a0 = s5.Points[s5.Points.Count - 1].Values[0].ToString("0");
            }

            chartData.ChartTitle = GetChartTitle();
            chartData.RealtimeInfo = string.Format("温度：{0}\nA0：{1}", temp, a0);

            chartData.AxesDictionary.Add(AxesName.Temperature.ToString(), axesTemp);
            chartData.AxesDictionary.Add(AxesName.AoValue.ToString(), axesAo);
            chartData.SeriesDictionary.Add(AxesName.Temperature.ToString(), dicSeriesTemp);
            chartData.SeriesDictionary.Add(AxesName.AoValue.ToString(), dicSeriesAo);

            chartData.Comment = GetComment(eHead, s1);

            chartData.ErrorData = Utility.GetSensorErrors(eHead, mSyncLast);

            return chartData;
        }

        private ChartDTO GetWasherChartDataInc(SensorDataHeadEntity eHead, DateTime lastReceivedDate)
        {
            SeriesDTO s1 = GetSeriesInc(eHead, SystemData.MeasurementSensor1, Properties.Resource.WasherSeries1, lastReceivedDate);
            SeriesDTO s2 = GetSeriesInc(eHead, SystemData.MeasurementSensor2, Properties.Resource.WasherSeries2, lastReceivedDate);
            SeriesDTO s3 = GetSeriesInc(eHead, SystemData.MeasurementSensor3, Properties.Resource.WasherSeries3, lastReceivedDate);
            SeriesDTO s4 = GetSeriesInc(eHead, SystemData.MeasurementSensor4, Properties.Resource.WasherSeries4, lastReceivedDate);
            SeriesDTO s5 = GetSeriesInc(eHead, SystemData.MeasurementSensor5, Properties.Resource.WasherSeries5, lastReceivedDate);

            if (s1 == null || s1.Points == null || s1.Points.Count == 0)
            {
                return null;
            }

            MergeWasherSensorData();

            Dictionary<string, SeriesDTO> dicSeriesTemp = new Dictionary<string, SeriesDTO>();
            dicSeriesTemp.Add(Properties.Resource.WasherSeries2, s2);
            Dictionary<string, SeriesDTO> dicSeriesAo = new Dictionary<string, SeriesDTO>();
            dicSeriesAo.Add(Properties.Resource.WasherSeries5, s5);
            dicSeriesAo.Add(Properties.Resource.WasherSeries1, s1);
            dicSeriesAo.Add(Properties.Resource.WasherSeries3, s3);
            dicSeriesAo.Add(Properties.Resource.WasherSeries4, s4);

            ChartDTO chartData = new ChartDTO();

            string temp = string.Empty;
            if (s2 != null && s2.Points.Count > 0)
            {
                temp = s2.Points[s2.Points.Count - 1].Values[0].ToString("#.0");
            }
            string a0 = string.Empty;
            if (s5 != null && s5.Points.Count > 0)
            {
                a0 = s5.Points[s5.Points.Count - 1].Values[0].ToString("0");
            }

            chartData.ChartTitle = GetChartTitle();
            chartData.RealtimeInfo = string.Format("温度：{0}\nA0：{1}", temp, a0);

            chartData.SeriesDictionary.Add(AxesName.Temperature.ToString(), dicSeriesTemp);
            chartData.SeriesDictionary.Add(AxesName.AoValue.ToString(), dicSeriesAo);

            chartData.Comment = GetComment(eHead, s1);

            chartData.ErrorData = Utility.GetSensorErrors(eHead, mSyncLast);

            return chartData;
        }

        private ChartDTO GetSterilizerChartData(SensorDataHeadEntity eHead)
        {
            SeriesDTO s1 = null;
            SeriesDTO s2 = null;
            SeriesDTO s3 = null;
            SeriesDTO s4 = null;
            SeriesDTO s5 = null;
            SeriesDTO s6 = null;

            if (mIsCompressed)
            {
                s1 = GetCompressedSeries(eHead, SystemData.MeasurementSensor1, Properties.Resource.SterilizerSeries1);
                s2 = GetCompressedSeries(eHead, SystemData.MeasurementSensor2, Properties.Resource.SterilizerSeries2);
                s3 = GetCompressedSeries(eHead, SystemData.MeasurementSensor4, Properties.Resource.SterilizerSeries3);
                s4 = GetCompressedSeries(eHead, SystemData.MeasurementSensor5, Properties.Resource.SterilizerSeries4);
                s5 = GetCompressedSeries(eHead, SystemData.MeasurementSensor6, Properties.Resource.SterilizerSeries5);
                s6 = GetCompressedSeries(eHead, SystemData.MeasurementSensor7, Properties.Resource.SterilizerSeries6);
            }
            else
            {
                s1 = GetSeries(eHead, SystemData.MeasurementSensor1, Properties.Resource.SterilizerSeries1);
                s2 = GetSeries(eHead, SystemData.MeasurementSensor2, Properties.Resource.SterilizerSeries2);
                s3 = GetSeries(eHead, SystemData.MeasurementSensor4, Properties.Resource.SterilizerSeries3);
                s4 = GetSeries(eHead, SystemData.MeasurementSensor5, Properties.Resource.SterilizerSeries4);
                s5 = GetSeries(eHead, SystemData.MeasurementSensor6, Properties.Resource.SterilizerSeries5);
                s6 = GetSeries(eHead, SystemData.MeasurementSensor7, Properties.Resource.SterilizerSeries6);
            }

            MergeSterilizerSensorData();

            AxesDTO axesTemp = new AxesDTO(AxesName.Temperature.ToString());
            axesTemp.AxisYProperty.Add(AxesDTO.AxisProperty.MinValueSerializable.ToString(), Properties.Resource.TempMinValue);
            axesTemp.AxisYProperty.Add(AxesDTO.AxisProperty.MaxValueSerializable.ToString(), Properties.Resource.TempMaxValue);
            axesTemp.AxisYProperty.Add(AxesDTO.AxisProperty.EndText.ToString(), Properties.Resource.TempUnit);

            AxesDTO axesPressure = new AxesDTO(AxesName.Pressure.ToString());
            if (!Properties.Resource.PressureMinValue.Equals(Properties.Resource.PressureMaxValue))
            {
                axesPressure.AxisYProperty.Add(AxesDTO.AxisProperty.MinValueSerializable.ToString(), Properties.Resource.PressureMinValue);
                axesPressure.AxisYProperty.Add(AxesDTO.AxisProperty.MaxValueSerializable.ToString(), Properties.Resource.PressureMaxValue);
            }
            axesPressure.AxisYProperty.Add(AxesDTO.AxisProperty.EndText.ToString(), Properties.Resource.PressureUnit);

            Dictionary<string, SeriesDTO> dicSeriesTemp = new Dictionary<string, SeriesDTO>();
            dicSeriesTemp.Add(Properties.Resource.SterilizerSeries1, s1);
            dicSeriesTemp.Add(Properties.Resource.SterilizerSeries2, s2);
            Dictionary<string, SeriesDTO> dicSeriesPressure = new Dictionary<string, SeriesDTO>();
            dicSeriesPressure.Add(Properties.Resource.SterilizerSeries3, s3);
            dicSeriesPressure.Add(Properties.Resource.SterilizerSeries4, s4);
            dicSeriesPressure.Add(Properties.Resource.SterilizerSeries5, s5);
            dicSeriesPressure.Add(Properties.Resource.SterilizerSeries6, s6);

            ChartDTO chartData = new ChartDTO();

            string temp = string.Empty;
            if (s2 != null && s2.Points.Count > 0)
            {
                temp = s2.Points[s2.Points.Count - 1].Values[0].ToString("#.0");
            }
            string p2 = string.Empty;
            if (s4 != null && s4.Points.Count > 0)
            {
                p2 = s4.Points[s4.Points.Count - 1].Values[0].ToString("0");
            }

            chartData.ChartTitle = GetChartTitle();
            chartData.RealtimeInfo = string.Format("温度：{0}\n压力：{1}", temp, p2);

            chartData.AxesDictionary.Add(AxesName.Temperature.ToString(), axesTemp);
            chartData.AxesDictionary.Add(AxesName.Pressure.ToString(), axesPressure);
            chartData.SeriesDictionary.Add(AxesName.Temperature.ToString(), dicSeriesTemp);
            chartData.SeriesDictionary.Add(AxesName.Pressure.ToString(), dicSeriesPressure);

            chartData.Comment = GetComment(eHead, s1);

            chartData.ErrorData = Utility.GetSensorErrors(eHead, mSyncLast);

            return chartData;
        }

        private ChartDTO GetSterilizerChartDataInc(SensorDataHeadEntity eHead, DateTime lastReceivedDate)
        {
            SeriesDTO s1 = GetSeriesInc(eHead, SystemData.MeasurementSensor1, Properties.Resource.SterilizerSeries1, lastReceivedDate);
            SeriesDTO s2 = GetSeriesInc(eHead, SystemData.MeasurementSensor2, Properties.Resource.SterilizerSeries2, lastReceivedDate);
            SeriesDTO s3 = GetSeriesInc(eHead, SystemData.MeasurementSensor4, Properties.Resource.SterilizerSeries3, lastReceivedDate);
            SeriesDTO s4 = GetSeriesInc(eHead, SystemData.MeasurementSensor5, Properties.Resource.SterilizerSeries4, lastReceivedDate);
            SeriesDTO s5 = GetSeriesInc(eHead, SystemData.MeasurementSensor6, Properties.Resource.SterilizerSeries5, lastReceivedDate);
            SeriesDTO s6 = GetSeriesInc(eHead, SystemData.MeasurementSensor7, Properties.Resource.SterilizerSeries6, lastReceivedDate);

            if (s1 == null || s1.Points == null || s1.Points.Count == 0)
            {
                return null;
            }

            MergeSterilizerSensorData();

            Dictionary<string, SeriesDTO> dicSeriesTemp = new Dictionary<string, SeriesDTO>();
            dicSeriesTemp.Add(Properties.Resource.SterilizerSeries1, s1);
            dicSeriesTemp.Add(Properties.Resource.SterilizerSeries2, s2);
            Dictionary<string, SeriesDTO> dicSeriesPressure = new Dictionary<string, SeriesDTO>();
            dicSeriesPressure.Add(Properties.Resource.SterilizerSeries3, s3);
            dicSeriesPressure.Add(Properties.Resource.SterilizerSeries4, s4);
            dicSeriesPressure.Add(Properties.Resource.SterilizerSeries5, s5);
            dicSeriesPressure.Add(Properties.Resource.SterilizerSeries6, s6);

            ChartDTO chartData = new ChartDTO();
            string temp = string.Empty;
            if (s2 != null && s2.Points.Count > 0)
            {
                temp = s2.Points[s2.Points.Count - 1].Values[0].ToString("#.0");
            }
            string p2 = string.Empty;
            if (s4 != null && s4.Points.Count > 0)
            {
                p2 = s4.Points[s4.Points.Count - 1].Values[0].ToString("0");
            }

            chartData.ChartTitle = GetChartTitle();
            chartData.RealtimeInfo = string.Format("温度：{0}\n压力：{1}", temp, p2);

            chartData.SeriesDictionary.Add(AxesName.Temperature.ToString(), dicSeriesTemp);
            chartData.SeriesDictionary.Add(AxesName.Pressure.ToString(), dicSeriesPressure);

            chartData.Comment = GetComment(eHead, s1);

            chartData.ErrorData = Utility.GetSensorErrors(eHead, mSyncLast);

            return chartData;
        }

        private string GetChartTitle()
        {
            string programCaption = string.Empty;
            string programName = string.Empty;
            if (mProgramData != null && mProgramData.Rows.Count > 0)
            {
                programName = mProgramData.Rows[mProgramData.Rows.Count -1][Properties.Resource.ProgramName].ToString();
            }
            if (!string.IsNullOrEmpty(programName))
            {
                programCaption = Business.Utility.GetProgramNameCaption(Sensor, programName);
            }

            return string.Format("{0}\n{1}", mSensor.SensorName, programCaption);

            //if (eHead == null)
            //{
            //    return mSensor.SensorKey;
            //}

            //if (mProgramData != null && mProgramData.Rows.Count > 0)
            //{
            //    DataRow row = mProgramData.Rows[mProgramData.Rows.Count - 1];
            //    string no = ParseHelper.ParseString(row[Properties.Resource.ProgramNo]);
            //    string name = ParseHelper.ParseString(row[Properties.Resource.ProgramName]);
            //    string phase = ParseHelper.ParseString(row[Properties.Resource.ProgramPhase]);

            //    string temp = string.Empty;
            //    temp = name;
            //    if (!string.IsNullOrEmpty(phase))
            //    {
            //        temp = temp + "," + phase;
            //    }

            //    if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Sterilizer)))
            //    {
            //        // 灭菌
            //        return string.Format("{0}号灭菌器: {1}", mSensor.SensorKey, temp);
            //    }
            //    else if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Washer)))
            //    {
            //        // 清洗
            //        return string.Format("{0}号清洗器: {1}", mSensor.SensorKey, temp);
            //    }
            //}

            //return mSensor.SensorKey;

        }

        private static string GetComment(SensorDataHeadEntity eHead, SeriesDTO dtoSeries)
        {
            if (eHead == null || !eHead.BeginDate.HasValue || dtoSeries == null || dtoSeries.Points == null || dtoSeries.Points.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("开始时间：");
            builder.Append("  ").AppendLine(eHead.BeginDate.Value.ToString("yyyy-MM-dd"));
            builder.Append("  ").AppendLine(eHead.BeginDate.Value.ToString("HH:mm:ss"));

            DateTime dtEnd = Convert.ToDateTime(dtoSeries.Points[dtoSeries.Points.Count - 1].Argument);
            //TimeSpan ts = dtEnd - eHead.BeginDate.Value;

            //builder.AppendLine("已用时间：");
            //builder.Append("  ").Append(ts.TotalMinutes.ToString("f2")).AppendLine("分");

            builder.AppendLine("更新时间：");
            builder.Append("  ").AppendLine(dtEnd.ToString("HH:mm:ss"));


            return builder.ToString();
        }

        private SeriesDTO GetCompressedSeries(SensorDataHeadEntity eHead, SystemData valueType, string seriesName)
        {
            DataRow[] rows = mSensorCompressedData.GetSensorSeriesValues(valueType.ToString());

            mDataDic.Add(seriesName, rows);

            return ToSeriesDTO(eHead, seriesName, rows);
        }

        private SeriesDTO GetSeries(SensorDataHeadEntity eHead, SystemData valueType, string seriesName)
        {
            SensorDataLineValueBll bllLineValue = new SensorDataLineValueBll();

            DataTable dt = bllLineValue.GetSensorSeriesValues(eHead.SensorDataHeadID.ToString().ToUpper(), valueType.ToString());

            DataRow[] rows;
            if (mSyncLast == null)
            {
                mSyncLast = Convert.ToDateTime(dt.Rows[dt.Rows.Count - 1]["ReceivedDate"]);
                rows = dt.Select();
            }
            else
            {
                rows = dt.Select("ReceivedDate <= '" + mSyncLast.Value.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'");
            }

            mDataDic.Add(seriesName, rows);

            return ToSeriesDTO(eHead, seriesName, rows);
        }

        private SeriesDTO GetSeriesInc(SensorDataHeadEntity eHead, SystemData valueType, string seriesName, DateTime lastReceivedDate)
        {
            SensorDataLineValueBll bllLineValue = new SensorDataLineValueBll();

            DataTable dt = bllLineValue.GetSensorSeriesValuesInc(eHead.SensorDataHeadID.ToString().ToUpper(), valueType.ToString(), lastReceivedDate);

            DataRow[] rows = null;
            if (dt != null && dt.Rows.Count > 0)
            {
                if (mSyncLast == null)
                {
                    mSyncLast = Convert.ToDateTime(dt.Rows[dt.Rows.Count - 1]["ReceivedDate"]);
                    rows = dt.Select();
                }
                else
                {
                    rows = dt.Select("ReceivedDate <= '" + mSyncLast.Value.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'");
                }

                mDataDic.Add(seriesName, rows);
            }
            else
            {
                mDataDic.Add(seriesName, null);
            }

            return ToSeriesDTO(eHead, seriesName, rows);
        }

        private SeriesDTO ToSeriesDTO(SensorDataHeadEntity eHead, string seriesName, DataRow[] rows)
        {
            if (rows != null && rows.Length > 0)
            {
                IList<SeriesPointDTO> lstPoints = new List<SeriesPointDTO>();
                SeriesPointDTO sp = null;

                int spCount = 0;
                DateTime? curSensorDate = null;
                DateTime? preSensorDate = null;
                double dif = 0;
                foreach (DataRow row in rows)
                {
                    long sts = Convert.ToInt64(row["SensorTimeSpan"]);

                    curSensorDate = ParseHelper.ParseToDateTime(row["SensorDate"]);

                    // 间隔
                    spCount++;
                    if (spCount > 1 && spCount < rows.Length)
                    {
                        dif = curSensorDate.Value.Subtract(preSensorDate.Value).TotalMilliseconds;

                        if (dif < mReadingInterval)
                        {
                            continue;
                        }
                    }

                    sp = new SeriesPointDTO();
                    sp.Argument = curSensorDate.Value.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    sp.Values = new double[] { Convert.ToDouble(row["ContentNumber"]) };
                    sp.ReceivedDate = Convert.ToDateTime(row["ReceivedDate"]);

                    lstPoints.Add(sp);

                    preSensorDate = curSensorDate;
                }

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

        private DataTable GetProgramData(SensorDataHeadEntity eHead)
        {
            DataRow[] rowsProgramNo = null;
            DataRow[] rowsProgramName = null;
            DataRow[] rowsProgramPhase = null;

            if (mIsCompressed)
            {
                rowsProgramNo = mSensorCompressedData.GetSensorSeriesValues(SystemData.ProgramNo.ToString());
                rowsProgramName = mSensorCompressedData.GetSensorSeriesValues(SystemData.ProgramName.ToString());
                rowsProgramPhase = mSensorCompressedData.GetSensorSeriesValues(SystemData.ProgramPhase.ToString());
            }
            else
            {

                SensorDataLineValueBll bllLineValue = new SensorDataLineValueBll();

                DataTable dtProgramNo = bllLineValue.GetSensorSeriesValues(eHead.SensorDataHeadID.ToString().ToUpper(), SystemData.ProgramNo.ToString());

                if (mSyncLast == null)
                {
                    mSyncLast = Convert.ToDateTime(dtProgramNo.Rows[dtProgramNo.Rows.Count - 1]["ReceivedDate"]);
                    rowsProgramNo = dtProgramNo.Select();
                }
                else
                {
                    rowsProgramNo = dtProgramNo.Select("ReceivedDate <= '" + mSyncLast.Value.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'");
                }

                DataTable dtProgramName = bllLineValue.GetSensorSeriesValues(eHead.SensorDataHeadID.ToString().ToUpper(), SystemData.ProgramName.ToString());
                rowsProgramName = dtProgramName.Select("ReceivedDate <= '" + mSyncLast.Value.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'");

                DataTable dtProgramPhase = bllLineValue.GetSensorSeriesValues(eHead.SensorDataHeadID.ToString().ToUpper(), SystemData.ProgramPhase.ToString());
                rowsProgramPhase = dtProgramPhase.Select("ReceivedDate <= '" + mSyncLast.Value.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'");
            }

            DataTable dtProgramData = new DataTable("ProgramData");
            dtProgramData.Columns.Add("ReceivedDate", typeof(DateTime));
            dtProgramData.Columns.Add("SensorDate", typeof(DateTime));
            dtProgramData.Columns.Add(Properties.Resource.ProgramNo, typeof(string));
            dtProgramData.Columns.Add(Properties.Resource.ProgramName, typeof(string));
            dtProgramData.Columns.Add(Properties.Resource.ProgramPhase, typeof(string));

            dtProgramData.BeginLoadData();
            for (int i = 0; i < rowsProgramNo.Length; i++)
            {

                DataRow row = dtProgramData.NewRow();

                row["ReceivedDate"] = rowsProgramNo[i]["ReceivedDate"];
                row["SensorDate"] = rowsProgramNo[i]["SensorDate"];
                row[Properties.Resource.ProgramNo] = rowsProgramNo[i]["ContentString"];
                row[Properties.Resource.ProgramName] = rowsProgramName[i]["ContentString"];
                row[Properties.Resource.ProgramPhase] = rowsProgramPhase[i]["ContentString"];

                dtProgramData.Rows.Add(row);
            }
            dtProgramData.EndLoadData();

            return dtProgramData;
        }

        private DataTable GetProgramDataInc(SensorDataHeadEntity eHead, DateTime lastReceivedDate)
        {
            SensorDataLineValueBll bllLineValue = new SensorDataLineValueBll();

            DataTable dtProgramNo = bllLineValue.GetSensorSeriesValuesInc(eHead.SensorDataHeadID.ToString().ToUpper(), SystemData.ProgramNo.ToString(), lastReceivedDate);

            if (dtProgramNo == null || dtProgramNo.Rows.Count == 0)
            {
                return null;
            }

            DataRow[] rowsProgramNo;
            if (mSyncLast == null)
            {
                mSyncLast = Convert.ToDateTime(dtProgramNo.Rows[dtProgramNo.Rows.Count - 1]["ReceivedDate"]);
                rowsProgramNo = dtProgramNo.Select();
            }
            else
            {
                rowsProgramNo = dtProgramNo.Select("ReceivedDate <= '" + mSyncLast.Value.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'");
            }

            DataTable dtProgramName = bllLineValue.GetSensorSeriesValuesInc(eHead.SensorDataHeadID.ToString().ToUpper(), SystemData.ProgramName.ToString(), lastReceivedDate);
            DataRow[] rowsProgramName = dtProgramName.Select("ReceivedDate <= '" + mSyncLast.Value.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'");

            DataTable dtProgramPhase = bllLineValue.GetSensorSeriesValuesInc(eHead.SensorDataHeadID.ToString().ToUpper(), SystemData.ProgramPhase.ToString(), lastReceivedDate);
            DataRow[] rowsProgramPhase = dtProgramPhase.Select("ReceivedDate <= '" + mSyncLast.Value.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'");

            DataTable dtProgramData = new DataTable("ProgramData");
            dtProgramData.Columns.Add("ReceivedDate", typeof(DateTime));
            dtProgramData.Columns.Add("SensorDate", typeof(DateTime));
            dtProgramData.Columns.Add(Properties.Resource.ProgramNo, typeof(string));
            dtProgramData.Columns.Add(Properties.Resource.ProgramName, typeof(string));
            dtProgramData.Columns.Add(Properties.Resource.ProgramPhase, typeof(string));

            dtProgramData.BeginLoadData();
            for (int i = 0; i < rowsProgramNo.Length; i++)
            {

                DataRow row = dtProgramData.NewRow();

                row["ReceivedDate"] = rowsProgramNo[i]["ReceivedDate"];
                row["SensorDate"] = rowsProgramNo[i]["SensorDate"];
                row[Properties.Resource.ProgramNo] = rowsProgramNo[i]["ContentString"];
                row[Properties.Resource.ProgramName] = rowsProgramName[i]["ContentString"];
                row[Properties.Resource.ProgramPhase] = rowsProgramPhase[i]["ContentString"];

                dtProgramData.Rows.Add(row);
            }
            dtProgramData.EndLoadData();

            return dtProgramData;
        }

    }
}
