using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace TestXY2
{
    class Program
    {
        static void Main(string[] args)
        {
        }
        private void QueryCSSDData()    //CSSD记录
        {
            string sql = string.Format(@" Select  dl.BEGINDATE,
                                                  d.DEVICEID,
                                                  d.DEVICENAME,
                                                  dl.TODAYUSETIMES,
                                                  dl.LogID,
                                                  dl.MonitorDataID,
		                                          CASE ISNULL(Convert(nvarchar(50),dl.MonitorDataID),'')  WHEN '' THEN '无记录' ELSE '查看监控数据' END AS MONITORLOG,
		                                          ou.FNames NAME,
		                                          tp.TYPENAME ISOK,
		                                          D.BARCODE as DEVICEBARCODE,
		                                          D.DEVICETYPE
                                              From  DevicesUseLog dl
                                              inner join Devices d  on dl.DeviceID = d.deviceid
                                              inner join AC_WorkGroup ou  on dl.UserID= ou.FID
                                              inner join SystemTypeParameter tp  on ( dl.isok = tp.TypeValue and tp.TypeKey = 'ContainerStatus')
                                              left join DeviceProcessRule prule  on dl.DeviceProcessRuleID = prule.RuleID
                                              Where 1=1");

            DataTable dt = GetTableBySQL(sql);
        }
        private static DataTable GetTableBySQL(string aSQL)
        {
            Infecon.CSSD.Business.BusinessBase bll = new Infecon.CSSD.Business.BusinessBase();
            DataTable dt = bll.ExecuteDataTable(aSQL);
            return dt;
        }
    }
}
