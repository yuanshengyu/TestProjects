using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Infecon.CSSD.Bll.Sensor;
using Infecon.CSSD.Business.Monitor;
using Infecon.CSSD.Entity.Sensor;
using Infecon.CSSD.Monitor.Common.SensorStatus;
using log4net;
using System.Drawing;
using System.Data;
using Infecon.Common.Utility;
using Infecon.CSSD.Monitor.Common.SensorError;
using Infecon.CSSD.Utility;

namespace Infecon.CSSD.Monitor.Belimed.Business
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
            SensorDataHeadEntity eHead = bllHead.GetNewestSensorDataHeadBySensorID(Sensor.SensorID.ToString().ToUpper());
            if (eHead == null)
            {
                SensorStatusDTO dtoStatus = new SensorStatusDTO();
                dtoStatus.SensorCaption = mSensor.SensorName;
                dtoStatus.SensorStatus = mSensor.PositionMark;
                return dtoStatus;
            }

            if (eHead.IsCompressed != null && eHead.IsCompressed.HasValue && eHead.IsCompressed.Value)
            {
                SensorStatusDTO dtoStatus = new SensorStatusDTO();
                dtoStatus.SensorCaption = mSensor.SensorName;
                dtoStatus.SensorStatus = mSensor.PositionMark;
                return dtoStatus;
            }

            if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Sterilizer)))
            {
                // 灭菌
                return GetStatusData(eHead);
            }
            else if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Washer)))
            {
                // 清洗
                return GetStatusData(eHead);
            }
            else
            {
                logger.WarnFormat("未处理的设备类型（[传感器：{0}，设备类型：{1}]）", Sensor.SensorKey, Sensor.SensorType);

                return null;
            }
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
                lstStatus.Add(new KeyValuePair<string, string>("程序名：", GetValue(dtoStatus.StatusData, Properties.Resource.ProgramNameCaption)));
                lstStatus.Add(new KeyValuePair<string, string>("程序段：", GetValue(dtoStatus.StatusData, SystemData.ProgramPhase.ToString())));
                lstStatus.Add(new KeyValuePair<string, string>("时间：", GetValue(dtoStatus.StatusData, Properties.Resource.UpdatedTime)));
                lstStatus.Add(new KeyValuePair<string, string>("温度：", GetValue(dtoStatus.StatusData, SystemData.MeasurementSensor2.ToString()) + " °C"));
                if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Sterilizer)))
                {
                    // 灭菌
                    lstStatus.Add(new KeyValuePair<string, string>("压力：", GetValue(dtoStatus.StatusData, SystemData.MeasurementSensor5.ToString()) + " mbar a"));
                }
                else if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Washer)))
                {
                    // 清洗
                    lstStatus.Add(new KeyValuePair<string, string>("A0：", GetValue(dtoStatus.StatusData, SystemData.MeasurementSensor5.ToString())));
                }
            }
            else
            {
                lstStatus.Add(new KeyValuePair<string, string>("程序名：", string.Empty));
                lstStatus.Add(new KeyValuePair<string, string>("程序段：", string.Empty));
                lstStatus.Add(new KeyValuePair<string, string>("时间：", string.Empty));
                lstStatus.Add(new KeyValuePair<string, string>("温度：", string.Empty));
                if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Sterilizer)))
                {
                    // 灭菌
                    lstStatus.Add(new KeyValuePair<string, string>("压力：", string.Empty));
                }
                else if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Washer)))
                {
                    // 清洗
                    lstStatus.Add(new KeyValuePair<string, string>("A0：", string.Empty));
                }
            }
            lstStatus.Add(new KeyValuePair<string, string>("状态：", Utility.GetStatusCaption(dtoStatus.SensorStatus)));

            return lstStatus;
        }

        public string GetStatusHtml(SensorStatusDTO dtoStatus)
        {
            if (dtoStatus == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(mHtmlTemplate))
            {
                string fileName = string.Empty;
                if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Sterilizer)))
                {
                    // 灭菌
                    fileName = Properties.Resource.SterilizerStatusTemplatePath; // @"Template\SterilizerStatus.html";
                }
                else if (Convert.ToInt32(Sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Washer)))
                {
                    // 清洗
                    fileName = Properties.Resource.WasherStatusTemplatePath; // @"Template\WasherStatus.html";
                }

                string dllPath = Assembly.GetExecutingAssembly().Location;
                string filePath = Path.Combine(Path.GetDirectoryName(dllPath), fileName);
                if (File.Exists(filePath))
                {
                    using (StreamReader sr = new StreamReader(filePath))
                    {
                        mHtmlTemplate = sr.ReadToEnd();
                    }
                }
                else
                {
                    logger.ErrorFormat("未找到指定的文件：{0}", filePath);
                    mHtmlTemplate = string.Empty;
                }
            }

            string sLines = mHtmlTemplate;
            if (!string.IsNullOrEmpty(sLines) && dtoStatus.StatusData.Count > 0)
            {
                sLines = Regex.Replace(sLines, @"<#\s*" + "SensorCaption" + @"\s*#>", dtoStatus.SensorCaption);

                sLines = Regex.Replace(sLines, @"<#\s*" + "ProgramNo" + @"\s*#>", GetValue(dtoStatus.StatusData, SystemData.ProgramNo.ToString()));
                sLines = Regex.Replace(sLines, @"<#\s*" + "ProgramName" + @"\s*#>", GetValue(dtoStatus.StatusData, SystemData.ProgramName.ToString()));
                sLines = Regex.Replace(sLines, @"<#\s*" + "ProgramPhase" + @"\s*#>", GetValue(dtoStatus.StatusData, SystemData.ProgramPhase.ToString()));
                sLines = Regex.Replace(sLines, @"<#\s*" + "ProgramNameCaption" + @"\s*#>", GetValue(dtoStatus.StatusData, Properties.Resource.ProgramNameCaption));

                sLines = Regex.Replace(sLines, @"<#\s*" + "MeasurementSensor1" + @"\s*#>", GetValue(dtoStatus.StatusData, SystemData.MeasurementSensor1.ToString()));
                sLines = Regex.Replace(sLines, @"<#\s*" + "MeasurementSensor2" + @"\s*#>", GetValue(dtoStatus.StatusData, SystemData.MeasurementSensor2.ToString()));
                sLines = Regex.Replace(sLines, @"<#\s*" + "MeasurementSensor3" + @"\s*#>", GetValue(dtoStatus.StatusData, SystemData.MeasurementSensor3.ToString()));
                sLines = Regex.Replace(sLines, @"<#\s*" + "MeasurementSensor4" + @"\s*#>", GetValue(dtoStatus.StatusData, SystemData.MeasurementSensor4.ToString()));
                sLines = Regex.Replace(sLines, @"<#\s*" + "MeasurementSensor5" + @"\s*#>", GetValue(dtoStatus.StatusData, SystemData.MeasurementSensor5.ToString()));
                sLines = Regex.Replace(sLines, @"<#\s*" + "MeasurementSensor6" + @"\s*#>", GetValue(dtoStatus.StatusData, SystemData.MeasurementSensor6.ToString()));
                sLines = Regex.Replace(sLines, @"<#\s*" + "MeasurementSensor7" + @"\s*#>", GetValue(dtoStatus.StatusData, SystemData.MeasurementSensor7.ToString()));

                sLines = Regex.Replace(sLines, @"<#\s*" + "ElapsedTime" + @"\s*#>", GetValue(dtoStatus.StatusData, Properties.Resource.ElapsedTime));
                sLines = Regex.Replace(sLines, @"<#\s*" + "UpdatedTime" + @"\s*#>", GetValue(dtoStatus.StatusData, Properties.Resource.UpdatedTime));

                string flg = GetBgImageFlg(dtoStatus);
                sLines = Regex.Replace(sLines, @"<#\s*" + "BgImageFlg" + @"\s*#>", flg);

                string statusCaption = Utility.GetStatusCaption(dtoStatus.SensorStatus);
                sLines = Regex.Replace(sLines, @"<#\s*" + "StatusCaption" + @"\s*#>", statusCaption);

                return sLines;
            }
            else
            {
                sLines = Regex.Replace(sLines, @"<#\s*" + "SensorCaption" + @"\s*#>", dtoStatus.SensorCaption);

                sLines = Regex.Replace(sLines, @"<#\s*" + "ProgramNo" + @"\s*#>", string.Empty);
                sLines = Regex.Replace(sLines, @"<#\s*" + "ProgramName" + @"\s*#>", string.Empty);
                sLines = Regex.Replace(sLines, @"<#\s*" + "ProgramPhase" + @"\s*#>", string.Empty);
                sLines = Regex.Replace(sLines, @"<#\s*" + "ProgramNameCaption" + @"\s*#>", string.Empty);

                sLines = Regex.Replace(sLines, @"<#\s*" + "MeasurementSensor1" + @"\s*#>", string.Empty);
                sLines = Regex.Replace(sLines, @"<#\s*" + "MeasurementSensor2" + @"\s*#>", string.Empty);
                sLines = Regex.Replace(sLines, @"<#\s*" + "MeasurementSensor3" + @"\s*#>", string.Empty);
                sLines = Regex.Replace(sLines, @"<#\s*" + "MeasurementSensor4" + @"\s*#>", string.Empty);
                sLines = Regex.Replace(sLines, @"<#\s*" + "MeasurementSensor5" + @"\s*#>", string.Empty);
                sLines = Regex.Replace(sLines, @"<#\s*" + "MeasurementSensor6" + @"\s*#>", string.Empty);
                sLines = Regex.Replace(sLines, @"<#\s*" + "MeasurementSensor7" + @"\s*#>", string.Empty);

                sLines = Regex.Replace(sLines, @"<#\s*" + "ElapsedTime" + @"\s*#>", string.Empty);
                sLines = Regex.Replace(sLines, @"<#\s*" + "UpdatedTime" + @"\s*#>", string.Empty);

                string flg = GetBgImageFlg(dtoStatus);
                sLines = Regex.Replace(sLines, @"<#\s*" + "BgImageFlg" + @"\s*#>", flg);

                string statusCaption = Utility.GetStatusCaption(dtoStatus.SensorStatus);
                sLines = Regex.Replace(sLines, @"<#\s*" + "StatusCaption" + @"\s*#>", statusCaption);

                return sLines;
            }

            return string.Empty;
        }

        //private static string GetStatusCaption(SensorStatusDTO dtoStatus)
        //{
        //    string statusCaption = string.Empty;
        //    switch (dtoStatus.SensorStatus)
        //    {
        //        case "On":
        //            statusCaption = "待机";
        //            break;
        //        case "Off":
        //            statusCaption = "关闭";
        //            break;
        //        case "Run":
        //            statusCaption = "运行";
        //            break;
        //        case "End":
        //            statusCaption = "结束";
        //            break;
        //        default:
        //            statusCaption = string.Empty;
        //            break;
        //    }
        //    return statusCaption;
        //}

        private static string GetBgImageFlg(SensorStatusDTO dtoStatus)
        {
            string flg = "Off";
            if (dtoStatus.SensorStatus == Enumerator.BelimedStatus.On.ToString()
                || dtoStatus.SensorStatus == Enumerator.BelimedStatus.Run.ToString()
                || dtoStatus.SensorStatus == Enumerator.BelimedStatus.End.ToString())
            {
                flg = "On";
            }
            else if (dtoStatus.ErrorData != null && dtoStatus.ErrorData.Count > 0)
            {
                flg = "Alert";
            }

            return flg;
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
                dtoStatus.ProgressMark = GetProgressMark(eHead, eLine);

                dtoStatus.ErrorData = Utility.GetSensorErrors(eHead, eLine.ReceivedDate);

                dtoStatus.SensorDate = eLine.SensorDate;
                dtoStatus.ReceivedDate = eLine.ReceivedDate;

                IList<SensorDataLineValueEntity> lstLineValue = helper.SelectList<SensorDataLineValueEntity>(string.Format("SensorDataLineID = '{0}'", eLine.SensorDataLineID.ToString().ToUpper()), string.Empty);

                if (lstLineValue != null && lstLineValue.Count > 0)
                {
                    string temp = string.Empty;

                    AddContentStringToStatus(dtoStatus, lstLineValue, SystemData.Status.ToString());
                    
                    // 程序
                    AddContentStringToStatus(dtoStatus, lstLineValue, SystemData.ProgramNo.ToString());
                    AddContentStringToStatus(dtoStatus, lstLineValue, SystemData.ProgramName.ToString());
                    AddContentStringToStatus(dtoStatus, lstLineValue, SystemData.ProgramPhase.ToString());
                    AddProgramNameCaption(dtoStatus);

                    // MeasurementSensor1 ~ MeasurementSensor7
                    AddContentNumberToStatus(dtoStatus, lstLineValue, SystemData.MeasurementSensor1.ToString());
                    AddContentNumberToStatus(dtoStatus, lstLineValue, SystemData.MeasurementSensor2.ToString());
                    AddContentNumberToStatus(dtoStatus, lstLineValue, SystemData.MeasurementSensor3.ToString());
                    AddContentNumberToStatus(dtoStatus, lstLineValue, SystemData.MeasurementSensor4.ToString());
                    AddContentNumberToStatus(dtoStatus, lstLineValue, SystemData.MeasurementSensor5.ToString());
                    AddContentNumberToStatus(dtoStatus, lstLineValue, SystemData.MeasurementSensor6.ToString());
                    AddContentNumberToStatus(dtoStatus, lstLineValue, SystemData.MeasurementSensor7.ToString());

                    // 已用时间
                    if (eLine.SensorDate != null && eLine.SensorDate.HasValue)
                    {
                        TimeSpan tsUsed = eLine.SensorDate.Value.Subtract(eHead.BeginDate.Value);
                        item = new StatusItemDTO(Properties.Resource.ElapsedTime, StatusItemDTO.StatusItemType.DoubleValue, tsUsed.TotalMinutes.ToString("f2"));
                        dtoStatus.StatusData.Add(item);
                    }

                    // 确定更新时间
                    if (eLine.SensorDate != null && eLine.SensorDate.HasValue)
                    {
                        item = new StatusItemDTO(Properties.Resource.UpdatedTime, StatusItemDTO.StatusItemType.DateTimeValue, eLine.SensorDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                        dtoStatus.StatusData.Add(item);
                    }
                }
            }

            return dtoStatus;
        }

        private Bitmap GetProgressMark(SensorDataHeadEntity eHead, SensorDataLineEntity eLine)
        {
            TimeSpan ts = eLine.SensorDate.Value.Subtract(eHead.BeginDate.Value);
            double mins = ts.TotalMinutes;
            string strPrefix = mins > 60 ? "DvMonitorOver_" : "DvMonitorIn_";
            string strImageName = string.Empty;

            if (mins > 60) mins = 55;            //超过60分钟的保持在55分钟状态
            if (mSensor.PositionMark == "End") mins = 60;  //结束时为60分钟状态
            if (mSensor.PositionMark == "On") mins = 0;
            int num = Convert.ToInt16(mins) / 5 * 5;
            strImageName = strPrefix + num.ToString();
            object obj = Infecon.CSSD.Resources.Images.ResourceManager.GetObject(
                    strImageName, Infecon.CSSD.Resources.Images.resourceCulture);
            if (obj != null)
            {
                return obj as Bitmap;
            }
            else
            {
                return null;
            }
        }

        private static void AddContentNumberToStatus(SensorStatusDTO dtoStatus, IList<SensorDataLineValueEntity> lstLineValue, string valueType)
        {
            var lst = lstLineValue.Where(lv => lv.ValueType == valueType);
            if (lst != null && lst.Count() > 0)
            {
                SensorDataLineValueEntity eLineValue = lst.First();
                if (eLineValue != null)
                {
                    string temp = string.Empty;
                    if (eLineValue.ContentNumber != null && eLineValue.ContentNumber.HasValue)
                    {
                        temp = eLineValue.ContentNumber.Value.ToString("0.00");
                    }
                    StatusItemDTO item = new StatusItemDTO(valueType, StatusItemDTO.StatusItemType.DoubleValue, temp);
                    dtoStatus.StatusData.Add(item);
                }
            }
        }

        private static void AddContentStringToStatus(SensorStatusDTO dtoStatus, IList<SensorDataLineValueEntity> lstLineValue, string valueType)
        {
            var lst = lstLineValue.Where(lv => lv.ValueType == valueType);
            if (lst != null && lst.Count() > 0)
            {
                SensorDataLineValueEntity eLineValue = lst.First();
                if (eLineValue != null)
                {
                    StatusItemDTO item = new StatusItemDTO(valueType, StatusItemDTO.StatusItemType.StringValue, eLineValue.ContentString);
                    dtoStatus.StatusData.Add(item);
                }
            }
        }

        private void AddProgramNameCaption(SensorStatusDTO dtoStatus)
        {
            string programCaption = string.Empty;
            string programName = GetValue(dtoStatus.StatusData, SystemData.ProgramName.ToString());
            if (!string.IsNullOrEmpty(programName))
            {
                programCaption = Business.Utility.GetProgramNameCaption(Sensor, programName);
            }
            StatusItemDTO item = new StatusItemDTO(Properties.Resource.ProgramNameCaption, StatusItemDTO.StatusItemType.StringValue, programCaption);
            dtoStatus.StatusData.Add(item);
        }

    }
}
