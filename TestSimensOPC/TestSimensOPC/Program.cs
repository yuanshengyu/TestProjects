using System;
using System.Collections.Generic;
using System.Text;
using OPCSiemensDAAutomation;

namespace TestSimensOPC
{
    class Program
    {
        static string hostServerName = "S7200SMART.OPCServer";
        static string hostIP = "127.0.0.1";
        static void Main(string[] args)
        {
            OPCServer kepServer = null;
            try
            {
                kepServer = new OPCServer();
            }
            catch(Exception ex1)
            {
                
            }
            kepServer.Connect(hostServerName, hostIP);

            if (kepServer.ServerState == (int)OPCServerState.OPCRunning)
            {
                Console.WriteLine("连接成功");
            }
            else
            {
                //这里你可以根据返回的状态来自定义显示信息，请查看自动化接口API文档
                Console.WriteLine("状态：" + kepServer.ServerState.ToString() + "   ");
            }
            Console.ReadLine();
        }
    }
}
