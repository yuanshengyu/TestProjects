using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Infecon.Common.Utility;
using Infecon.CSSD.Bll.Sensor;
using Infecon.CSSD.Business.Monitor;
using Infecon.CSSD.Entity.Sensor;
using Infecon.CSSD.Monitor.Common.SensorChart;
using log4net;
using Infecon.CSSD.Entity;
using Infecon.CSSD.Business;

namespace Infecon.CSSD.Monitor.Belimed.Business
{
    public class SensorChartOldHelper : ISensorChart
    {
        //日志记录
        protected static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private MonitorCollectDataEntity mHeadEntity;
        private IList<MonitorCollectDataEntryEntity> mDetailEntity;

        private SensorEntity mSensor;
        private int mReadingInterval;

        //private Dictionary<string, DataRow[]> mDataDic;
        //private DataTable mSensorData;

        private bool mIsCompressed = false;

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
                return string.Empty;
            }
        }

        public bool IsNew
        {
            get;
            set;
        }

        public ChartDTO GetChartData()
        {
            return null;
        }

        public ChartDTO GetChartData(string sdhID)
        {
            IsNew = true;

            SensorOldHelper helper = new SensorOldHelper();
            MonitorCollectDataEntity eHead = helper.SelectSingle<MonitorCollectDataEntity>(string.Format("FID = {0}", sdhID), string.Empty);
            mHeadEntity = eHead;
            mDetailEntity = null;

            if (eHead == null)
            {
                return null;
            }
            else
            {
                if (eHead.FHasCompress != null && eHead.FHasCompress == 1)
                {
                    // 已压缩
                    mIsCompressed = true;
                }

                return GetChartDataByHead(eHead);
            }
        }

        public ChartDTO GetChartDataInc(DateTime? lastReceivedDate, bool canChangeHead)
        {
            return null;
        }

        public DataTable GetSensorData()
        {
            DataTable dtSensorData = new DataTable("SensorData");
            dtSensorData.Columns.Add("序号", typeof(int));
            dtSensorData.Columns.Add("时间", typeof(string));
            //dtSensorData.Columns.Add("程序名", typeof(string));
            dtSensorData.Columns.Add("程序段", typeof(string));

            if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Sterilizer)))
            {
                dtSensorData.Columns.Add(Properties.Resource.SterilizerSeries1, typeof(decimal));
                dtSensorData.Columns.Add(Properties.Resource.SterilizerSeries2, typeof(decimal));
                dtSensorData.Columns.Add(Properties.Resource.SterilizerSeries3, typeof(decimal));
                dtSensorData.Columns.Add(Properties.Resource.SterilizerSeries4, typeof(decimal));
                dtSensorData.Columns.Add(Properties.Resource.SterilizerSeries5, typeof(decimal));
                dtSensorData.Columns.Add(Properties.Resource.SterilizerSeries6, typeof(decimal));
            }
            else if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Washer)))
            {
                dtSensorData.Columns.Add(Properties.Resource.WasherSeries1, typeof(decimal));
                dtSensorData.Columns.Add(Properties.Resource.WasherSeries2, typeof(decimal));
                dtSensorData.Columns.Add(Properties.Resource.WasherSeries3, typeof(decimal));
                dtSensorData.Columns.Add(Properties.Resource.WasherSeries4, typeof(decimal));
                dtSensorData.Columns.Add(Properties.Resource.WasherSeries5, typeof(decimal));
            }
            dtSensorData.Columns.Add("错误", typeof(string));

            if (mDetailEntity != null)
            {
                int index = 1;
                foreach (MonitorCollectDataEntryEntity de in mDetailEntity)
                {
                    DataRow row = dtSensorData.NewRow();
                    row["序号"] = index++;
                    row["时间"] = de.FDate.Value.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    row["程序段"] = de.FPhaseText;
                    if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Sterilizer)))
                    {
                        row[Properties.Resource.SterilizerSeries1] = de.FValue1;
                        row[Properties.Resource.SterilizerSeries2] = de.FValue2;
                        row[Properties.Resource.SterilizerSeries3] = de.FValue3;
                        row[Properties.Resource.SterilizerSeries4] = de.FValue4;
                        row[Properties.Resource.SterilizerSeries5] = de.FValue5;
                        row[Properties.Resource.SterilizerSeries6] = de.FValue6;
                    }
                    else if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Washer)))
                    {
                        row[Properties.Resource.WasherSeries1] = de.FValue1;
                        row[Properties.Resource.WasherSeries2] = de.FValue2;
                        row[Properties.Resource.WasherSeries3] = de.FValue3;
                        row[Properties.Resource.WasherSeries5] = de.FValue4;
                        row[Properties.Resource.WasherSeries4] = de.FValue5;
                    }
                    row["错误"] = de.FErrorText;

                    dtSensorData.Rows.Add(row);
                }
            }

            return dtSensorData;
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
                lstHead.Add(new KeyValuePair<string, string>("批次", mHeadEntity.FBatchNo.ToString()));
                lstHead.Add(new KeyValuePair<string, string>(Common.Consts.C_RETURN_PREFIX, string.Empty));
                lstHead.Add(new KeyValuePair<string, string>("程序名", mHeadEntity.FPargramName));
                lstHead.Add(new KeyValuePair<string, string>("程序段", mHeadEntity.FProgramPhrase));
                lstHead.Add(new KeyValuePair<string, string>(Common.Consts.C_RETURN_PREFIX, string.Empty));
                if (mHeadEntity.FBeginDate != null && mHeadEntity.FBeginDate.HasValue)
                {
                    lstHead.Add(new KeyValuePair<string, string>("开始时间", mHeadEntity.FBeginDate.Value.ToString("yyyy-MM-dd HH:mm:ss")));
                }
                else
                {
                    lstHead.Add(new KeyValuePair<string, string>("开始时间", string.Empty));
                }
                if (mHeadEntity.FEndDate != null && mHeadEntity.FEndDate.HasValue)
                {
                    lstHead.Add(new KeyValuePair<string, string>("结束时间", mHeadEntity.FEndDate.Value.ToString("yyyy-MM-dd HH:mm:ss")));
                }
                else
                {
                    lstHead.Add(new KeyValuePair<string, string>("结束时间", string.Empty));
                }
                lstHead.Add(new KeyValuePair<string, string>(Common.Consts.C_RETURN_PREFIX, string.Empty));

                if (mDetailEntity != null)
                {
                    IList<string> lstAnalysis = GetAnalysisInfo(mDetailEntity, Properties.Resource.CheckedField);
                    if (lstAnalysis != null)
                    {
                        lstHead.Add(new KeyValuePair<string, string>("最高温度", lstAnalysis[0]));
                        lstHead.Add(new KeyValuePair<string, string>("134度以上持续时间", lstAnalysis[1]));
                        lstHead.Add(new KeyValuePair<string, string>(Common.Consts.C_RETURN_PREFIX, string.Empty));
                        lstHead.Add(new KeyValuePair<string, string>("结果", lstAnalysis[2]));
                    }
                }

                return lstHead;
            }
        }

        public Dictionary<string, IList<SensorShowPropertyEntity>> GetShowProperty()
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

        #endregion

        private static IList<string> GetAnalysisInfo(IList<MonitorCollectDataEntryEntity> dt, string propertyName)
        {
            try
            {
                DataTable dtInfo = Common.Utility.CreateDataTableByEntity<MonitorCollectDataEntryEntity>();
                Common.Utility.FillDataTableByEntity<MonitorCollectDataEntryEntity>(ref dtInfo, dt);

                List<string> lt = new List<string>();
                //最高温度
                dtInfo.DefaultView.Sort = propertyName + " DESC";
                lt.Add(dtInfo.DefaultView.ToTable().Rows[0][propertyName].ToString());

                //134度持续时间
                dtInfo.DefaultView.Sort = "FDate DESC";
                dtInfo.DefaultView.RowFilter = propertyName + ">=134";
                DataTable dtTemp = dtInfo.DefaultView.ToTable();
                DateTime dt1 = DateTime.Parse(dtTemp.Rows[0]["FDate"].ToString());
                DateTime dt2 = DateTime.Parse(dtTemp.Rows[dtTemp.Rows.Count - 1]["FDate"].ToString());
                TimeSpan ts = dt1 - dt2;
                lt.Add(DateTime.Parse(ts.ToString()).ToString("m:ss") + "分");

                string Result = AnalysisData(dt, propertyName) ? "合格" : "不合格";
                lt.Add(Result);
                return lt;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static bool AnalysisData(IList<MonitorCollectDataEntryEntity> DetailAnalysis, string propertyName)
        {
            double temp = 0;
            DateTime? deCur, deEnd = null; bool IsStart = false;

            foreach (MonitorCollectDataEntryEntity dr in DetailAnalysis)
            {
                temp = double.Parse(Common.Utility.GetValueByProperty(dr, propertyName).ToString());
                if (temp >= 134 && !IsStart)
                {
                    IsStart = true;
                    deCur = dr.FDate.Value;
                    deEnd = deCur.Value.AddMinutes(4);
                }
                else if (temp < 134 && IsStart)
                {
                    IsStart = false;
                }
                else if (temp > 134)
                {
                    deCur = dr.FDate.Value;
                    if (deCur.Value >= deEnd.Value)
                    {
                        return true;//合格
                    }
                }
            }
            return false;// "不合格";
        }

        private ChartDTO GetChartDataByHead(MonitorCollectDataEntity eHead)
        {
            MonitorHelper oldHelper = new MonitorHelper();
            IList<MonitorCollectDataEntryEntity> lstDetail = oldHelper.GetMonitorData(eHead.FID, mIsCompressed);
            mDetailEntity = lstDetail;

            if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Sterilizer)))
            {
                // 灭菌
                return GetSterilizerChartData(eHead, lstDetail);
            }
            else if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Washer)))
            {
                // 清洗
                return GetWasherChartData(eHead, lstDetail);
            }
            else
            {
                logger.WarnFormat("未处理的设备类型（[传感器：{0}，设备类型：{1}]）", Sensor.SensorKey, Sensor.SensorType);

                return null;
            }

            //return null;
        }

        private ChartDTO GetWasherChartData(MonitorCollectDataEntity eHead, IList<MonitorCollectDataEntryEntity> lstDetail)
        {
            SeriesDTO s1 = GetSeries(lstDetail, "FValue1", Properties.Resource.WasherSeries1);  //Med
            SeriesDTO s2 = GetSeries(lstDetail, "FValue2", Properties.Resource.WasherSeries2);  //CT
            SeriesDTO s3 = GetSeries(lstDetail, "FValue3", Properties.Resource.WasherSeries3);  //CDNo
            SeriesDTO s5 = GetSeries(lstDetail, "FValue4", Properties.Resource.WasherSeries5);  //Ao
            SeriesDTO s4 = GetSeries(lstDetail, "FValue5", Properties.Resource.WasherSeries4);  //DosV

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

            chartData.ChartTitle = GetChartTitle(eHead);

            chartData.AxesDictionary.Add(AxesName.Temperature.ToString(), axesTemp);
            chartData.AxesDictionary.Add(AxesName.AoValue.ToString(), axesAo);
            chartData.SeriesDictionary.Add(AxesName.Temperature.ToString(), dicSeriesTemp);
            chartData.SeriesDictionary.Add(AxesName.AoValue.ToString(), dicSeriesAo);

            chartData.Comment = GetComment(eHead, s1);

            //chartData.ErrorData = Utility.GetSensorErrors(eHead, mSyncLast);

            return chartData;
        }

        private ChartDTO GetSterilizerChartData(MonitorCollectDataEntity eHead, IList<MonitorCollectDataEntryEntity> lstDetail)
        {
            SeriesDTO s1 = GetSeries(lstDetail, "FValue1", Properties.Resource.SterilizerSeries1);
            SeriesDTO s2 = GetSeries(lstDetail, "FValue2", Properties.Resource.SterilizerSeries2);
            SeriesDTO s3 = GetSeries(lstDetail, "FValue3", Properties.Resource.SterilizerSeries3);
            SeriesDTO s4 = GetSeries(lstDetail, "FValue4", Properties.Resource.SterilizerSeries4);
            SeriesDTO s5 = GetSeries(lstDetail, "FValue5", Properties.Resource.SterilizerSeries5);
            SeriesDTO s6 = GetSeries(lstDetail, "FValue6", Properties.Resource.SterilizerSeries6);

            AxesDTO axesTemp = new AxesDTO(AxesName.Temperature.ToString());
            axesTemp.AxisYProperty.Add(AxesDTO.AxisProperty.MinValueSerializable.ToString(), Properties.Resource.TempMinValue);
            axesTemp.AxisYProperty.Add(AxesDTO.AxisProperty.MaxValueSerializable.ToString(), Properties.Resource.TempMaxValue);
            axesTemp.AxisYProperty.Add(AxesDTO.AxisProperty.EndText.ToString(), Properties.Resource.TempUnit);

            AxesDTO axesPressure = new AxesDTO(AxesName.Pressure.ToString());
            if (Properties.Resource.PressureMinValue.Equals(Properties.Resource.PressureMaxValue))
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
            chartData.ChartTitle = GetChartTitle(eHead);
            chartData.AxesDictionary.Add(AxesName.Temperature.ToString(), axesTemp);
            chartData.AxesDictionary.Add(AxesName.Pressure.ToString(), axesPressure);
            chartData.SeriesDictionary.Add(AxesName.Temperature.ToString(), dicSeriesTemp);
            chartData.SeriesDictionary.Add(AxesName.Pressure.ToString(), dicSeriesPressure);

            chartData.Comment = GetComment(eHead, s1);

            return chartData;
        }

        private string GetChartTitle(MonitorCollectDataEntity eHead)
        {
            string programCaption = string.Empty;
            string programName = eHead.FPargramName;

            if (!string.IsNullOrEmpty(programName))
            {
                programCaption = Business.Utility.GetProgramNameCaption(Sensor, programName);
            }

            return string.Format("{0}\n{1}", mSensor.SensorName, programCaption);
        }

        private static string GetComment(MonitorCollectDataEntity eHead, SeriesDTO dtoSeries)
        {
            if (eHead == null || !eHead.FBeginDate.HasValue || dtoSeries == null || dtoSeries.Points == null || dtoSeries.Points.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("开始日期：");
            builder.Append("  ").AppendLine(eHead.FBeginDate.Value.ToString("yyyy-MM-dd"));
            builder.Append("  ").AppendLine(eHead.FBeginDate.Value.ToString("HH:mm:ss"));

            DateTime dtEnd = Convert.ToDateTime(dtoSeries.Points[dtoSeries.Points.Count - 1].Argument);
            TimeSpan ts = dtEnd - eHead.FBeginDate.Value;

            builder.AppendLine("消耗总时间：");
            builder.Append("  ").Append(ts.TotalMinutes.ToString("f2")).AppendLine("分");

            return builder.ToString();
        }

        private SeriesDTO GetSeries(IList<MonitorCollectDataEntryEntity> lstDetail, string propertyName, string seriesName)
        {
            return ToSeriesDTO(seriesName, lstDetail, propertyName);
        }

        private SeriesDTO ToSeriesDTO(string seriesName, IList<MonitorCollectDataEntryEntity> lstDetail, string propertyName)
        {
            if (lstDetail != null && lstDetail.Count > 0)
            {
                IList<SeriesPointDTO> lstPoints = new List<SeriesPointDTO>();
                SeriesPointDTO sp = null;

                int spCount = 0;
                DateTime? curSensorDate = null;
                DateTime? preSensorDate = null;
                double dif = 0;
                foreach (MonitorCollectDataEntryEntity row in lstDetail)
                {
                    curSensorDate = row.FDate;

                    // 间隔
                    spCount++;
                    if (spCount > 1 && spCount < lstDetail.Count)
                    {
                        dif = curSensorDate.Value.Subtract(preSensorDate.Value).TotalMilliseconds;

                        if (dif < mReadingInterval)
                        {
                            continue;
                        }
                    }

                    sp = new SeriesPointDTO();
                    sp.Argument = curSensorDate.Value.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    sp.Values = new double[] { Convert.ToDouble(Common.Utility.GetValueByProperty(row, propertyName)) };
                    sp.ReceivedDate = curSensorDate;

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

    }
}
