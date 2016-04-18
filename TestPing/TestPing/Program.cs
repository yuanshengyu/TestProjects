using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace TestPing
{
    class Program
    {
        static void Main(string[] args)
        {
            
            Console.ReadLine();
        }
        static void func3()
        {
            DateTime dt1 = DateTime.Now;
            Ping ping = new Ping();
            PingReply reply = ping.Send("192.168.1.230", 1000);
            DateTime dt2 = DateTime.Now;
            TimeSpan ts = dt2 - dt1;
            Console.WriteLine(string.Format("{2} 用时：{0}ms {1}",ts.TotalMilliseconds, ts.ToString(), reply.Status));
        }
        static void func2()
        {
            NETRESOURCE myNetResource = new NETRESOURCE();
            myNetResource.dwScope = 2;
            myNetResource.dwType = 1;//0
            myNetResource.dwDispalyType = 3;
            myNetResource.dwUsage = 1;
            myNetResource.LocalName = "X:";
            myNetResource.RemoteName = "\\\\192.168.1.120\\data";
            uint nret = WNetAddConnection2(myNetResource, "Infecon123", "Administrator", 1);
            Console.WriteLine(nret);
        }
    　[DllImport("mpr.dll", EntryPoint = "WNetAddConnection2")]
        public static extern uint WNetAddConnection2(
            [In] NETRESOURCE lpNetResource,
            string lpPassword,
            string lpUsername,
            uint dwFlags);
 
        [DllImport("Mpr.dll")]
        public static extern uint WNetCancelConnection2(
            string lpName,
            uint dwFlags,
            bool fForce);
        [StructLayout(LayoutKind.Sequential)]
        public class NETRESOURCE
        {
            public int dwScope;
            public int dwType;
            public int dwDispalyType;
            public int dwUsage;
            public string LocalName;
            public string RemoteName;
            public string Comment;
            public string Provider;
        }
        static void func()
        {
            Ping ping = new Ping();
            PingReply reply = ping.Send("192.168.1.130", 120);
            if (reply.Status == IPStatus.Success)
            {
                Console.WriteLine("IP通");
            }
            else
            {
                Console.WriteLine("IP不通");
            }
        }

        static string Getdatabasename(string connectionstring)
        {
            //"Provider=Microsoft.Jet.OleDb.4.0;Data Source=D:\SterDB.mdb"
            int index = connectionstring.LastIndexOf("=");
            if (index > 0 && connectionstring.Length > index + 1)
            {
                return connectionstring.Substring(index + 1);
            }
            return "";
        }
    }
}
