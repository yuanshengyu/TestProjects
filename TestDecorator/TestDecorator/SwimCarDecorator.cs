using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestDecorator
{
    public class SwimCarDecorator : CarDecorator
    {
        public SwimCarDecorator(Car car)
            : base(car)
        {

        }
        public override void show()
        {
            this.getCar().show();
            Console.WriteLine("可以游");
        }
    }
}
