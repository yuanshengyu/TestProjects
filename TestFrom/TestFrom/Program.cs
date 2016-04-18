using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestFrom
{
    class Program
    {
        static void Main(string[] args)
        {
            func2();
            Console.ReadLine();

        }
        static void func2()
        {
            DateTime dt = DateTime.Now;
            int year = dt.Year;
            int month = dt.Month;
            int day = dt.Day;
            int hour = dt.Hour;
            int min = dt.Minute;
            int sec = dt.Second;
            Console.WriteLine(string.Format("{0}-{1}-{2} {3}:{4}:{5}", year, month, day, hour, min, sec));
        }
        static void func1()
        {
            int[] nums = { 1, 2, 3, 4, 5, 6, 7 };
            var num = from a in nums
                      where a < 6 & a > 2
                      select a;
            foreach (int b in num)
            {
                Console.WriteLine(b);
            }
        }
    }
}
