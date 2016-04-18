using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;

namespace TestListSort3
{
    class Program
    {
        static void Main(string[] args)
        {
            Info i1 = new Info();
            i1.ID = 2;
            i1.Name = "i1";
            Info i2 = new Info();
            i2.ID = 5;
            i2.Name = "i2";
            Info i3 = new Info();
            i3.ID = 3;
            i3.Name = "i3";
            List<Info> lists = new List<Info>();
            lists.Add(i1);
            lists.Add(i2);
            lists.Add(i3);
            Info.ListSort(lists, "Name", "desc");
            foreach (Info i in lists)
            {
                Console.WriteLine(i.Name);
            }
            Console.ReadLine();
        }

    }
    public class Info
    {
        public int ID { get; set; }
        public string Name { get; set; }
        static public void ListSort(List<Info>infos,string field, string rule)
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