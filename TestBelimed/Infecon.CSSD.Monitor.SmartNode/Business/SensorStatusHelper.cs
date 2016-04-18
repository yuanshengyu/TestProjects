using System;
using System.Collections.Generic;
using Infecon.CSSD.Bll.Sensor;
using Infecon.CSSD.Business.Monitor;
using Infecon.CSSD.Entity.Sensor;
using Infecon.CSSD.Monitor.Common.SensorStatus;
using log4net;

namespace Infecon.CSSD.Monitor.SmartNode.Business
{
    class SensorStatusHelper : ISensorStatus
    {
        //日志记录
        protected static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private SensorEntity mSensor;

        private string mHtmlTemplate = string.Empty;

        // 同步最后的时间结点
        //private DateTime? mSyncLast;

        #region ISensorStatus 成员

        public SensorEntity Sensor
        {
            get { return mSensor; }
            set { mSensor = value; }
        }

        public SensorStatusDTO GetStatusData()
        {
            // 刷新状态
            SensorHelper<object> helper = new SensorHelper<object>();
            mSensor = helper.SelectSingle<SensorEntity>(string.Format("SensorID = '{0}'", mSensor.SensorID.ToString().ToUpper()), string.Empty);
            
            SensorDataHeadBll bllHead = new SensorDataHeadBll();
            SensorDataHeadEntity eHead = bllHead.GetNewestSensorDataHeadBySensorID(Sensor.SensorID.ToString().ToUpper().ToUpper());
            if (eHead == null)
            {
                SensorStatusDTO dtoStatus = new SensorStatusDTO();
                dtoStatus.SensorCaption = mSensor.SensorName;
                return dtoStatus;
            }

            if (eHead.IsCompressed != null && eHead.IsCompressed.HasValue && eHead.IsCompressed.Value)
            {
                SensorStatusDTO dtoStatus = new SensorStatusDTO();
                dtoStatus.SensorCaption = mSensor.SensorName;
                return dtoStatus;
            }

            //if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Sterilizer)))
            //{
            //}
            //else if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Washer)))
            //{
            //}
            //else
            //{
            //    logger.WarnFormat("未处理的设备类型（[传感器：{0}，设备类型：{1}]）", Sensor.SensorKey, Sensor.SensorType);

            //    return null;
            //}

            return GetStatusData(eHead);
        }

        public IList<KeyValuePair<string,string>> GetStatusList(SensorStatusDTO dtoStatus)
        {
            if (dtoStatus == null)
            {
                return null;
            }

            IList<KeyValuePair<string, string>> lstStatus = new List<KeyValuePair<string, string>>();
            if (dtoStatus.StatusData.Count > 0)
            {
                lstStatus.Add(new KeyValuePair<string, string>("时间：", GetValue(dtoStatus.StatusData, Properties.Resource.UpdatedTime)));

                if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Temperature))
                || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperatureHumidity))
                || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperaturePressure))
                || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperaturePressureAccurate)))
                {
                    // 温度
                    lstStatus.Add(new KeyValuePair<string, string>("温度：", GetValue(dtoStatus.StatusData, Properties.Resource.TemperatureSeries) + " " + Properties.Resource.TemperatureUnit));
                }

                if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Humidity))
                    || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperatureHumidity)))
                {
                    // 湿度
                    lstStatus.Add(new KeyValuePair<string, string>("湿度：", GetValue(dtoStatus.StatusData, Properties.Resource.HumiditySeries) + " " + Properties.Resource.HumidityUnit));
                }

                if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Pressure))
                    || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperaturePressure))
                    || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperaturePressureAccurate)))
                {
                    // 气压
                    lstStatus.Add(new KeyValuePair<string, string>("气压：", GetValue(dtoStatus.StatusData, Properties.Resource.PressureSeries) + " " + Properties.Resource.PressureUnit));
                }
                // 电压
                lstStatus.Add(new KeyValuePair<string, string>("电压：", GetValue(dtoStatus.StatusData, Properties.Resource.VoltageSeries) + " " + Properties.Resource.VoltageUnit));

            }
            else
            {
                lstStatus.Add(new KeyValuePair<string, string>("时间：", string.Empty));

                if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Temperature))
                || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperatureHumidity))
                || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperaturePressure))
                || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperaturePressureAccurate)))
                {
                    // 温度
                    lstStatus.Add(new KeyValuePair<string, string>("温度：", string.Empty));
                }

                if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Humidity))
                    || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperatureHumidity)))
                {
                    // 湿度
                    lstStatus.Add(new KeyValuePair<string, string>("湿度：", string.Empty));
                }

                if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Pressure))
                    || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperaturePressure))
                    || Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.TemperaturePressureAccurate)))
                {
                    // 气压
                    lstStatus.Add(new KeyValuePair<string, string>("气压：", string.Empty));
                }
                // 电压
                lstStatus.Add(new KeyValuePair<string, string>("电压：", string.Empty));

            }

            return lstStatus;
        }

        public string GetStatusHtml(SensorStatusDTO dtoStatus)
        {
            return string.Empty;
        }

        #endregion

        private string GetValue(IList<StatusItemDTO> lstItem, string caption)
        {
            foreach (StatusItemDTO item in lstItem)
            {
                if (item.Caption == caption)
                {
                    if (item.Value != null)
                    {
                        return item.Value.ToString();
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
            return string.Empty;
        }

        private SensorStatusDTO GetStatusData(SensorDataHeadEntity eHead)
        {
            SensorStatusDTO dtoStatus = new SensorStatusDTO();

            dtoStatus.SensorCaption = mSensor.SensorName;
            dtoStatus.SensorStatus = mSensor.PositionMark;

            StatusItemDTO item = null;

            SensorHelper<object> helper = new SensorHelper<object>();

            SensorDataLineEntity eLine = helper.SelectSingle<SensorDataLineEntity>(string.Format("SensorDataHeadID = '{0}'", eHead.SensorDataHeadID.ToString().ToUpper()), "ReceivedDate DESC");

            if (eLine != null)
            {
                dtoStatus.SensorDate = eLine.SensorDate;
                dtoStatus.ReceivedDate = eLine.ReceivedDate;

                // Temperature
                AddValueNumberToStatus(dtoStatus, eLine.ValueNumber1, Properties.Resource.TemperatureSeries);
                AddValueNumberToStatus(dtoStatus, eLine.ValueNumber2, Properties.Resource.PressureSeries);
                AddValueNumberToStatus(dtoStatus, eLine.ValueNumber3, Properties.Resource.HumiditySeries);
                AddValueNumberToStatus(dtoStatus, eLine.ValueNumber4, Properties.Resource.VoltageSeries);

                // 确定更新时间
                if (eLine.SensorDate != null && eLine.SensorDate.HasValue)
                {
                    item = new StatusItemDTO(Properties.Resource.UpdatedTime, StatusItemDTO.StatusItemType.DateTimeValue, eLine.SensorDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                    dtoStatus.StatusData.Add(item);
                }
            }

            return dtoStatus;
        }

        private static void AddValueNumberToStatus(SensorStatusDTO dtoStatus, decimal? valueNumber, string caption)
        {
            string temp = string.Empty;
            if (valueNumber != null && valueNumber.HasValue)
            {
                temp = valueNumber.Value.ToString("0.00");
                StatusItemDTO item = new StatusItemDTO(caption, StatusItemDTO.StatusItemType.StringValue, temp);
                dtoStatus.StatusData.Add(item);
            }
        }

    }
}
