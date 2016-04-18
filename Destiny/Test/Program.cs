using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            string line = "第1卦乾为天（乾卦）刚健中正";
            //Match s = Regex.Match(line, @"第(\d{1,2})卦([(].*[)])(.*)");
            Match s = Regex.Match(line, @"第(\d{1,2})卦(.*）)(.*)");
        }
    }
}
