using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestDecorator
{
    public abstract class CarDecorator : Car
    {
        private Car car;
        public Car getCar()
        {
            return car;
        }
        public CarDecorator(Car car)
        {
            this.car = car;
        }
        public abstract void show();
    }
}
