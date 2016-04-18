using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestDict
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, int> dicts = new Dictionary<string, int>();
            dicts.Add("name1", 2);
            dicts.Add("name2", 3);
            Console.WriteLine(dicts["name2"]);
            Console.ReadLine();
        }
    }
}
