using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Data;
using System.Text.RegularExpressions;
using System.Globalization;

namespace TestCSV
{
    class Program
    {
        static void Main(string[] args)
        {
            func6();
            Console.ReadLine();
        }
        static void func6()
        {
            string str = "\"Cycle Name\"";
            if (Regex.IsMatch(str.ToUpper(), @"CYCLE.*NAME"))
            {
                Console.WriteLine(true);
            }
        }
        static void func5()
        {
            string str = "KC_1_123.csv";
            try
            {
                Match s = Regex.Match(str, @"([KC])_(\d{1,})_(\d{1,})[.]csv");
                Console.WriteLine(s.Groups[0]);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        static void func4()
        {
            string str = "2014 04 25 01:23:45";
            DateTime dt = Convert.ToDateTime(str);
            DateTime dt2 = DateTime.Now;
            TimeSpan span = dt - dt2;
            Console.WriteLine(span.Days.ToString());
        }
        static void func3(){
            FileInfo fi = new FileInfo(@"D:\倍力曼\datafile\WASHER02");
            string filePath = @"D:\倍力曼\datafile\WASHER02";
            foreach (string f in Directory.GetFiles(filePath))
            {
                CSVFileHelper.LoadFile(f);
                DataTable dt = CSVFileHelper.GetDataTable();
                CSVFileHelper.CloseFile();
                string co = "";
                foreach (DataColumn c in dt.Columns)
                {
                    co += c.Caption.PadRight(10, ' ');

                }
                Console.WriteLine("Column:" + co);
                string ro = "";
                foreach (DataRow r in dt.Rows)
                {
                    ro = "";
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        ro += r[i].ToString().PadRight(8, ' ');
                    }
                    Console.WriteLine("Row:" + ro);
                }
                break;
            }

            Console.ReadLine();
        }
        static void func2()
        {
            string filePath = @"D:\倍力曼\datafile\WASHER02";
            foreach (string f in Directory.GetFiles(filePath)){
                ChangePath(new FileInfo(f));
            }
        }
        static void ChangePath(FileInfo fi)
        {
            string dir = Path.GetDirectoryName(fi.FullName);
            dir += @"\bak";
            File.Move(fi.FullName, dir + @"\" + fi.Name);
        }
        static void func()
        {
            string filePath = @"D:\倍力曼\datafile\WASHER02";
            foreach (string f in Directory.GetFiles(filePath))
            {
                //FileStream fs = new FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                StreamReader sr = new StreamReader(f, Encoding.Default);
                while (true)
                {
                    try
                    {
                        string line = sr.ReadLine();
                        Console.WriteLine(line);
                        Thread.Sleep(1000);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.ReadLine();
                    }
                }
                Console.WriteLine("end");
                Console.ReadLine();
            }
        }
    }
}
