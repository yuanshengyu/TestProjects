using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestListSort2
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
            var query = from items in lists orderby items.Name select items;
            foreach (Info i in query)
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
        public int CompareTo(object obj)
        {
            try
            {
                Info info1 = obj as Info;
                if (this.ID > info1.ID) return 0;
                return 1;
            }
            catch
            {
                throw;
            }
        }
    }
}
