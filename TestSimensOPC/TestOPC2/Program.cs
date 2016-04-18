using System;
using System.Collections.Generic;
using System.Text;

using OPC.Common;
using OPC.Data.Interface;
using OPC.Data;

namespace TestOPC2
{
    class Program
    {
        static string serverName = "S7200SMART.OPCServer";
        static void Main(string[] args)
        {
            try
            {
                OpcServer server = new OpcServer();
                server.Connect(serverName);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }
    }
}
