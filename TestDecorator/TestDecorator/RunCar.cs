using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestDecorator
{
    public class RunCar : Car
    {
        public void show()
        {
            Console.WriteLine("可以跑");
        }
    }
}
