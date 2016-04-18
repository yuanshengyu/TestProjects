using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestOPCShinva
{
    public class MetaMonitor
    {
        public MetaMonitor()
        { }

        public int DeviceType { get; set; }
        public string DeviceID { get; set; }
        public string DeviceName { get; set; }
        public string Barcode { get; set; }
        public int ProgramNo { get; set; }  //P1,2...P9,P10
        public string ProgramName { get; set; }
        public int Phase { get; set; }
        public int BatchNO { get; set; }    //运行次数
        public decimal FDataIndex { get; set; }    //运行次数
        public string PhaseName { get; set; }
        public decimal FID { get; set; }
        public decimal PFID { get; set; }
        public string FLogID { get; set; }
        public int index { get; set; }
        public DateTime BeginDate { get; set; } //启动时间
        public DateTime EndDate { get; set; }   //结束时间
        public DateTime EndDate1 { get; set; }
        public DateTime LastDate { get; set; }
        public DateTime FDate { get; set; }    //细节数据
        public string DeviceValidMsg { get; set; }
        public string ShinvaKEPSever { get; set; }
        public string year { get; set; }
        public string month { get; set; }
        public string date { get; set; }
        public string hour { get; set; }
        public string minute { get; set; }
        public string second { get; set; }
        public double FValue1 { get; set; }
        public double FValue2 { get; set; }
        public double FValue3 { get; set; }
        public double FValue4 { get; set; }
        public double FValue5 { get; set; }
        public double FValue6 { get; set; }
        public string FStatus { get; set; }
 
    }
}
