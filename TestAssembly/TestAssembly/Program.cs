using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace TestAssembly
{
    class Program
    {
        static void Main(string[] args)
        {
            Assembly assembly = Assembly.Load("ClassLibrary1");
            Type type = assembly.GetType("ClassLibrary1.Class1");
            object obj = assembly.CreateInstance("ClassLibrary1.Class1");
            FieldInfo[] fis = type.GetFields();
            foreach (FieldInfo fi in fis)
            {
                fi.SetValue(obj, "NameValue");
            }
        }
    }
}
