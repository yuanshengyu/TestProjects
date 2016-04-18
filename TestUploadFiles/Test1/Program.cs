using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

using HTTPHelper;

namespace Test1
{
    class Program
    {
        static void Main(string[] args)
        {
            NameValueCollection data = new NameValueCollection();
            data.Add("name", "木子屋");
            data.Add("url", "http://www.mzwu.com/");
            Console.WriteLine(HttpHelper.HttpUploadFile("http://localhost/Test", new string[] { @"E:\Index.htm", @"E:\test.rar" }, data));

            Console.ReadKey();
        }
    }
}
