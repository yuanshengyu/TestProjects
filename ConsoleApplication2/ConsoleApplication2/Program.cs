using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Ports;
using Infecon.CSSD.Entity;
using Infecon.CSSD.Business;
using Infecon.CSSD.Entity.Monitor;
using System.Configuration;
using System.Data;
using System.Data.OracleClient;
using Infecon.Common.Utility;
using System.Text.RegularExpressions;
using System.Threading;
using TestReflection;

namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {
            func();
            Console.ReadLine();
        }
        static void func()
        {
            int MatchMonitorTime = 30;
            DateTime BeginDate = Convert.ToDateTime("2015-09-18 16:18:29");
            DateTime aMatchBeginDate = BeginDate.AddMinutes(-1 * MatchMonitorTime);
            DateTime aMatchEndDate = BeginDate.AddMinutes(MatchMonitorTime);
            string SQL = ("select Top 1 logid from DevicesUseLog where BeginDate>='{0}' and BeginDate<='{1}' and DeviceID = '{2}' order by begindate desc");//, aMatchBeginDate, aMatchEndDate,'3400E711-F6D2-47B7-AF05-2583B3F669CC');
            SQL = string.Format(SQL,aMatchBeginDate, aMatchEndDate,"3400E711-F6D2-47B7-AF05-2583B3F669CC");
            DataTable dt = DBProvider.ExecuteDataTable(SQL);
            Console.WriteLine(dt.Rows[0][0]);
        }
        static void InsertStatus()
        {
            //string[] barcodes = { "11509", "11510", "11511", "11512", "11513", "11514", "11515", "11516" };
            string[] barcodes2 = { "11114", "11115", "11116"};
            foreach(string str in barcodes2)
            {
                string SQL = string.Format("insert into devicesstatus (fdeviceid, fcurrentstatus, frecordid) values('{0}','On',0)", str);
                int i = DBProvider.ExecuteNonQuery(SQL);
                if (i > 0)
                {
                    Console.WriteLine("插入成功！");
                }
                else
                {
                    Console.WriteLine("失败");
                }
            }
        }
        static void Search()
        {
            string SQL = "select Top 10 * from monitorcollectdata order by fbegindate desc";
            string SQL2 = "select * from devicesstatus";
            DataTable dt = DBProvider.ExecuteDataTable(SQL);
            List<CollectDataEntity> lcs = DBHelper.GetEntity<CollectDataEntity>(dt);
            foreach (CollectDataEntity cde in lcs)
            {
                //Console.WriteLine(string.Format("FID: {0}; FDeviceID:{1}; FBeginDate:{2]; FzoneNo:{3}",cde.FID, cde.FDeviceID,cde.FBeginDate,0));
                Console.WriteLine(string.Format("FID: {0}, FDeviceID: {1}, FBeginDate: {2}, FzoneNo:{3}", cde.FID, cde.FDeviceID, cde.FBeginDate, cde.FZoneNo));
            }
            //int count = 0;
            //foreach (DataRow dr in dt.Rows)
            //{
            //    count++;
            //    foreach (object obj in dr.ItemArray)
            //    {
            //        Console.Write(obj.ToString() + "   ");
            //    }
            //    Console.WriteLine("");
            //    if (count == 10)
            //    {
            //        break;
            //    }
            //}
        }
        static void MoveData()
        {
            string SQL2 = @"update MONITORCOLLECTDATAENTRY set fvalue5 = fvalue2 where fparentid = ''";

            
            int i = DBProvider.ExecuteNonQuery(SQL2);
            Console.WriteLine(i.ToString());
            Console.ReadLine();
        }
        static void func11()
        {
            float f1 = Convert.ToSingle("2.34");
            Console.WriteLine(f1.ToString());
        }
        static void func1(){
            string SQL2 = @"insert into MONITORCOLLECTDATA(FID,
                                                              FLogID,
                                                              FDeviceID,
                                                              FMonitorType,
                                                              FDataSourceType,
                                                              FBeginDate,
                                                              FEndDate,
                                                              FSystemNo,
                                                              FVersion,
                                                              FProgramNo,

                                                              FPargramName,
                                                              FProgramPhrase,
                                                              FBatchNo,
                                                              FHasCompress)
 
                                                      Values ({0},'{1}','{2}',{3},{4},to_date('{5}','yyyy-mm-dd HH24:MI:SS'),
to_date('{6}','yyyy-mm-dd HH24:MI:SS'),0,0,0,'灭菌','0',{7},0)";
                SQL2 = string.Format(SQL2, 1667,
                                         "00000000-0000-0000-0000-000000000000",
                                         "11513",//barcode传给fdeviceid?
                                         4,
                                         0,
                                         "2013-12-23 12:13:45",
                                         "2013-12-24 12:13:45",
                                         10131224121345
                                      );

                string SQL = @"insert into MONITORCOLLECTDATA(FID,
                                                              FLogID,
                                                              FDeviceID,
                                                              FMonitorType,
                                                              FDataSourceType,
                                                              FBeginDate,
                                                              FHasCompress)
 
                                                      Values ({0},'{1}','{2}',{3},{4},to_date('{5}','yyyy-mm-dd HH24:MI:SS'),0)";
                SQL = string.Format(SQL, 1667,
                                         "00000000-0000-0000-0000-000000000000",
                                         "11513",//barcode传给fdeviceid?
                                         4,
                                         0,
                                         "2013-12-23 12:13:45"
                                      );
                int i = DBProvider.ExecuteNonQuery(SQL);
            Console.WriteLine(i.ToString());
            Console.ReadLine();
        }
    }
    
}
