using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestDecorator
{
    class Program
    {
        static void Main(string[] args)
        {
            RunCar rcar = new RunCar();
            rcar.show();
            Car scar = new SwimCarDecorator(rcar);
            scar.show();
            Car fcar = new FlyCarDecorator(scar);
            fcar.show();
            Console.ReadKey();
        }
    }
}
