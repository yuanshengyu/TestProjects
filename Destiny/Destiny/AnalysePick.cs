using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace Destiny
{
    public class AnalysePick
    {
        static public Future GetFuture(int pick)
        {
            StreamReader sr = new StreamReader("Destiny.txt", Encoding.Default);
            Future result = new Future();
            try
            {
                while (true)
                {
                    string line = sr.ReadLine();
                    if (line != null)
                    {
                        Match s = Regex.Match(line, @"第(\d{1,2})卦(.*）)(.*)");
                        int count = s.Groups.Count;
                        if (count == 4)
                        {
                            int rpick = Convert.ToInt32(s.Groups[1].ToString().Trim());
                            if (rpick != pick)
                            {
                                continue;
                            }
                            result.Pick = rpick;
                            result.Name = s.Groups[2].ToString().Trim();
                            result.SubName = s.Groups[3].ToString().Trim();
                            line = sr.ReadLine();
                            result.ShortCharge = line.Trim();
                            line = sr.ReadLine();
                            result.LongCharge = line.Trim();
                            line = sr.ReadLine();
                            result.LongCharge += " "+line.Trim();
                            for (int i = 0; i < 6;i++)
                            {
                                line = sr.ReadLine();
                                Match s2 = Regex.Match(line, @"(.{2})：(.*)");
                                int count2 = s2.Groups.Count;
                                if (count2 == 3)
                                {
                                    string str = s2.Groups[1].ToString().Trim();
                                    if (str == "事业")
                                    {
                                        result.Cause = s2.Groups[2].ToString().Trim();
                                    }
                                    else if (str == "经商")
                                    {
                                        result.Business = s2.Groups[2].ToString().Trim();
                                    }
                                    else if (str == "求名")
                                    {
                                        result.Fame = s2.Groups[2].ToString().Trim();
                                    }
                                    else if (str == "外出")
                                    {
                                        result.GoOut = s2.Groups[2].ToString().Trim();
                                    }
                                    else if (str == "婚恋")
                                    {
                                        result.Love = s2.Groups[2].ToString().Trim();
                                    }
                                    else if (str == "决策")
                                    {
                                        result.Decision = s2.Groups[2].ToString().Trim();
                                    }
                                }
                            }
                            return result;
                        }
                    }
                    else
                    {
                        return null;
                    } 
                }
            }
            catch
            {
                return null;
            }
        }
    }
    public class Future
    {
        public int Pick;
        public string Name;
        public string SubName;
        public string ShortCharge;
        public string LongCharge;
        public string Cause;
        public string Business;
        public string Fame;
        public string GoOut;
        public string Love;
        public string Decision;
    }
}
