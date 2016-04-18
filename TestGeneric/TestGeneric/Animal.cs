using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestGeneric
{
    public abstract class Animal
    {
        protected string name;
        public Animal(string name)
        {
            this.name = name;
        }
        public abstract void MakeNoise();
    }
    public class Chicken : Animal
    {
        public Chicken(string name)
            : base(name)
        {

        }
        public override void MakeNoise()
        {
            Console.WriteLine(name+": Cluck!");
        }
    }
    public class Cow : Animal
    {
        public Cow(string name)
            : base(name)
        {

        }
        public override void MakeNoise()
        {
            Console.WriteLine(name+": Moo!");
        }
    }
    public class SuperCow : Cow
    {
        public SuperCow(string name)
            : base(name)
        {

        }
        public void Fly()
        {
            Console.WriteLine(name+" is Flying!");
        }
        public override void MakeNoise()
        {
            Console.WriteLine(name+": Moo!I am supercow!");
        }
    }
}
