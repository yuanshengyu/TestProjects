using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace TestRegex
{
    class Program
    {
        static void Main(string[] args)
        {
            func4();
            Console.ReadLine();
        }
        static void func4()
        {
            string line = " COMPLETE AT  9:09:53A  ";
            Match m = Regex.Match(line, @"(.+)\s+(\d+:\d+:\d+)(\w)?");
            int count = m.Groups.Count;
            for (int i = 0; i < count; i++)
            {
                string st2r = m.Groups[i].ToString();
                Console.WriteLine(st2r);
            }
        }
        static void func3()
        {
            string str = "15:29:02       079.0     -082.9";
            Match m = Regex.Match(str, @"(\d+:\d+:\d+)\s+(\d+[.]*\d*)[^\d]*?(-*\d+[.]*\d*)");
            int count = m.Groups.Count;
            for (int i = 0; i < count; i++)
            {
                string st2r = m.Groups[i].ToString();
                Console.WriteLine(st2r);
            }
        }
        static void func2()
        {
            string str = "15:59:43   022.0";
            Match s = Regex.Match(str, @"(\d{1,}:\d{2}:\d{2}) *(\d{1,}[.]*\d*)");
            int count = s.Groups.Count;
            string[] strs = new string[count];
            for (int i = 0; i < count; i++)
            {
                strs[i] = s.Groups[i].ToString();
                Console.WriteLine(strs[i]);
            }
        }
        static void func1()
        {
            string str = "P9 程序 启动时间:15-10-20 15:57:15 运行次数:01085";
            Match s = Regex.Match(str, @"(.+)程序.*(\d{2}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}).*运行次数:(\d{1,})");
            int count = s.Groups.Count;
            string[] strs = new string[count];
            for (int i = 0; i < count; i++)
            {
                strs[i] = s.Groups[i].ToString();
                Console.WriteLine(strs[i]);
            }
            Console.WriteLine(Convert.ToDateTime(strs[2]));
        }
    }
}
