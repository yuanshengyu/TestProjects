using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TestSharedDisk
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = @"Y:";
            string[] dirs = Directory.GetDirectories(path);
            foreach (string dir in dirs)
            {
                Console.WriteLine(dir);
            }
            Console.ReadLine();
        }
    }
}
