using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Testone
{
     class Class1:Class2
    {
        readonly int a = 10;
        public Class1()
        {
            a = 12;
        }
        public override void func1()
        {
            Console.WriteLine("class1.func1"+a);
        }
    }
}
