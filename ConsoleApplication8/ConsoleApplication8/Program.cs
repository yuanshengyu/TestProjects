using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication8
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime dt1 = DateTime.Now.AddHours(-1);
            DateTime dt2 = DateTime.Now;
            TimeSpan ts = dt2-dt1;
            Console.WriteLine(ts.TotalMinutes);
            Console.ReadLine();
        }
    }
}
