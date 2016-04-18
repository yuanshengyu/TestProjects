using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestBytesToASCII
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string>strs = new List<string>{ "12", "23", "34" };
            string str2 = "23";
            if(strs.Contains(str2))
            {
                Console.WriteLine("true");
                
            }
            Console.ReadLine();
            return;
            byte[] bytes = { 0x99,0xbf};
            byte[] bytes2 = { 0x41};
            string str = Encoding.Default.GetString(bytes);
            int index = str.IndexOf("134");
            if (index > 0)
            {
                str = str.Substring(0, index) + "134℃";
            }
            Console.WriteLine(str);
            Console.ReadLine();
        }
    }
}
