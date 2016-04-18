using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infecon.CSSD.Entity.Sensor;
using Infecon.CSSD.Business.Monitor;
using Infecon.CSSD.Entity;
using Infecon.CSSD.Bll.Sensor;
using System.Data;
using Infecon.CSSD.Monitor.Common.SensorStatus;
using Infecon.Common.Utility;
using Infecon.CSSD.Monitor.Common.SensorError;

namespace Infecon.CSSD.Monitor.Belimed.Business
{
    class Utility
    {
        public static string GetProgramNameCaption(SensorEntity sensor, string programName)
        {

            int idParent = 0;
            if (Convert.ToInt32(sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Sterilizer)))
            {
                // 灭菌
                idParent = 13;

            }
            else if (Convert.ToInt32(sensor.SensorType).Equals(Convert.ToInt32(Common.Consts.SensorType.Washer)))
            {
                // 清洗
                idParent = 14;
            }
            else
            {
                return string.Empty;
            }

            SensorHelper<object> helper = new SensorHelper<object>();
            AssistDataEntity entity = helper.SelectSingle<AssistDataEntity>("FParentID = " + idParent.ToString() + " and fkey = '" + programName + "'", string.Empty);
            if (entity == null)
            {
                return programName;
            }
            else
            {
                return entity.FCaption;
            }
        }

        public static IList<ErrorItemDTO> GetSensorErrors(SensorDataHeadEntity eHead, DateTime? dtSyncLast)
        {
            SensorDataLineValueBll bllLineValue = new SensorDataLineValueBll();

            DataTable dt = bllLineValue.GetSensorSeriesValues(eHead.SensorDataHeadID.ToString().ToUpper(), SystemData.ErrorNo.ToString());

            if (dt != null)
            {
                DataRow[] rows;
                if (dtSyncLast == null && !dtSyncLast.HasValue)
                {
                    rows = dt.Select();
                }
                else
                {
                    rows = dt.Select("ReceivedDate <= '" + dtSyncLast.Value.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'");
                }

                IList<ErrorItemDTO> lstError = new List<ErrorItemDTO>();
                foreach (DataRow row in rows)
                {
                    ErrorItemDTO item = new ErrorItemDTO();
                    item.BeginDateTime = ParseHelper.ParseToDateTime(row["SensorDate"]);
                    item.ErrorNo = ParseHelper.ParseString(row["ContentString"]);

                    lstError.Add(item);
                }

                return lstError;
            }

            return null;
        }

        public static string GetStatusCaption(string stauts)
        {
            string statusCaption = string.Empty;
            switch (stauts)
            {
                case "On":
                    statusCaption = "待机";
                    break;
                case "Off":
                    statusCaption = "关闭";
                    break;
                case "Run":
                    statusCaption = "运行";
                    break;
                case "End":
                    statusCaption = "结束";
                    break;
                default:
                    statusCaption = string.Empty;
                    break;
            }
            return statusCaption;
        }
    }
}
