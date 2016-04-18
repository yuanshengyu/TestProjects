using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestDecorator
{
    public class FlyCarDecorator : CarDecorator
    {
        public FlyCarDecorator(Car car)
            : base(car)
        {

        }
        public override void show()
        {
            this.getCar().show();
            Console.WriteLine("可以飞");
        }
    }
}
