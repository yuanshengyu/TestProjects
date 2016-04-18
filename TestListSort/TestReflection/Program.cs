using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace TestReflection
{
    class Program
    {
        static void Main(string[] args)
        {
            Info i1 = new Info();
            i1.ID = 2;
            i1.Name = "i1";
            Type type = i1.GetType();
            PropertyInfo[] pis = type.GetProperties();
            foreach (PropertyInfo pi in pis)
            {
                Console.WriteLine(pi.Name);
            }
            Console.ReadLine();
        }
    }
    public class Info
    {
        public int ID { get; set; }
        public string Name { get; set; }
        static public void ListSort(List<Info> infos, string field, string rule)
        {
            infos.Sort(
                delegate(Info info1, Info info2)
                {
                    Type type = info1.GetType();
                    PropertyInfo pinfo = type.GetProperty(field);
                    if (rule == "asc")
                    {
                        return pinfo.GetValue(info1, null).ToString().CompareTo(pinfo.GetValue(info2, null).ToString());
                    }
                    else
                    {
                        return pinfo.GetValue(info2, null).ToString().CompareTo(pinfo.GetValue(info1, null).ToString());
                    }
                }
                );
        }
    }
}
