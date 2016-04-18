using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestExpression
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            string englist = "This is an apple.";
            string chinese = Translator.Translate(englist);
            Console.WriteLine(chinese);
            Console.ReadKey();
        }
    }
}
