using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestGeneric
{
    class Program
    {
        static void Main(string[] args)
        {
            func3();
            Console.ReadLine();
        }
        static void func1()
        {
            GenericClass<String, int> a = new GenericClass<string, int>();
            Console.WriteLine(a.GetString());
        }
        static void func2()
        {
            Farm<Animal> farm = new Farm<Animal>();
            farm.Animals.Add(new Cow("CowOne"));
            farm.Animals.Add(new Chicken("ChickenOne"));
            farm.Animals.Add(new Chicken("ChickenTwo"));
            farm.Animals.Add(new SuperCow("SuperCowOne"));
            farm.MakeNoises();
        }
        static void func3()
        {
            Farm<Animal> farm = new Farm<Animal>();
            farm.Animals.Add(new Cow("CowOne"));
            farm.Animals.Add(new Chicken("ChickenOne"));
            farm.Animals.Add(new Chicken("ChickenTwo"));
            farm.Animals.Add(new SuperCow("SuperCowOne"));
            Farm<Animal> farm2 = new Farm<Animal>();
            farm2.Animals.Add(new Cow("CowTwo"));
            farm2.Animals.Add(new Chicken("ChickenOne"));
            farm2.Animals.Add(new Chicken("ChickenThree"));
            farm2.Animals.Add(new SuperCow("SuperCowTwo"));
            Farm<Cow> farm1 = new Farm<Cow>();
            farm1.Animals.Add(new Cow("Cow1"));
            Farm<Animal> farm3 = farm + farm1;
            Farm<Animal> farm4 = farm3.GetSpecies<Cow>();
            farm4.MakeNoises();
        }
    }
}
