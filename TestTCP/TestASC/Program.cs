using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace TestASC
{
    class Program
    {
        static void Main(string[] args)
        {
            //31 35 31 30 31 36 31 31 30 38 33 34
            //151015110834
            string str = "12   23 34 45 56 67 78 89";
            byte[] bytes = GetBytes(str);
            Console.ReadLine();
        }
        static byte[] GetBytes(string str)
        {
            string[] strs = str.Split(' ');
            List<byte> lbytes = new List<byte>();
            foreach (string s in strs)
            {
                if (s.Trim() != "")
                {
                    try
                    {
                        byte b = Convert.ToByte(s.Trim());
                        lbytes.Add(b);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            return lbytes.ToArray();
        }
    }
}
