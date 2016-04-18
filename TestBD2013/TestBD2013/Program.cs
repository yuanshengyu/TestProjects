using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace TestBD2013
{
    class Program
    {
        [DllImport("C:\\InfeconBDtest\\BDTestDll.dll", EntryPoint = "ComOpen", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int ComOpen();

        [DllImport("C:\\InfeconBDtest\\BDTestDll.dll", EntryPoint = "BDTestRun", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int BDTestRun();

        [DllImport("C:\\InfeconBDtest\\BDTestDll.dll", EntryPoint = "BDTestBarcode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int BDTestBarcode();

        [DllImport("C:\\InfeconBDtest\\BDTestDll.dll", EntryPoint = "BDTestStamp", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int BDTestStamp(int dwFlags);

        [DllImport("C:\\InfeconBDtest\\BDTestDll.dll", EntryPoint = "BDTestDll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int BDTestDll();
        static void Main(string[] args)
        {
            int result = BDTestRun();
            Console.WriteLine(BDMessage(result));
            Console.ReadLine();
        }
        private static string BDMessage(int MsgID)
        {
            string message = "";
            switch (MsgID)
            {
                case 0:
                    message = "OK";
                    break;
                case 1:
                    message = "数据线未连接或驱动程序安装不正确！";
                    break;
                case 2:
                    message = "请打开电源开关！";
                    break;
                case 3:
                    message = "相机损坏，请联系感信售后：400-600-9453！";
                    break;
                case 4:
                    message = "二级判断失败！";
                    break;
                case 5:
                    message = "试纸合格！";
                    break;
                case 6:
                    message = "二级判断不合格！";
                    break;
                case 7:
                    message = "条形码读取失败 ！";
                    break;
                case 8:
                    message = "请先校准图像！";
                    break;
                case 9:
                    message = "请先进行试纸判定！";
                    break;
                case 10:
                    message = "一级判断不合格！";
                    break;
                case 11:
                    message = "未注册！";
                    break;
                case 13:
                    message = "请正确放置试纸";
                    break;
                case 14:
                    message = "试纸未使用！";
                    break;
                case 15:
                    message = "读取条码成功！";
                    break;
                default:
                    break;
            }

            return message;
        }
    }
}
