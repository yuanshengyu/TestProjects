using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestTreeNode2
{
    class Program
    {
        static void Main(string[] args)
        {
            string node = "";
            TreeMap map = new TreeMap();
            Random rd = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < 10; i++)
            {
                int data = rd.Next(0, 27);
                char a = (char)(data + 'a');
                map.Add(a, "123");
            }
            Console.ReadLine();
        }
    }
}
