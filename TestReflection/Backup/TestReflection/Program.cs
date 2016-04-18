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
            ClassHelper ch = new ClassHelper();
            foreach (object pi in ch.GetType().GetCustomAttributes(false))
            {
                if (pi is HelpAttribute)
                {
                    Console.WriteLine(pi.ToString());
                }
            }
            PropertyInfo[] pis = ch.GetType().GetProperties();
            ch.GetType().
            foreach (PropertyInfo pi in pis)
            {
                Console.WriteLine(pi.Name);
                object[] pas = pi.GetCustomAttributes(false);
                foreach (object obj in pas)
                {
                    Console.WriteLine(((HelpAttribute)obj).Url);
                }
                pi.Se
            }
            Console.ReadLine();
        }
    }
}
