using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestGeneric
{
    public class Farm<T>:IEnumerable<T>
        where T:Animal
    {
        private List<T> animals = new List<T>();
        public List<T> Animals
        {
            get
            {
                return animals;
            }
        }
        public IEnumerator<T> GetEnumerator()
        {
            return animals.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return animals.GetEnumerator();
        }
        public void MakeNoises()
        {
            foreach (T animal in animals)
            {
                animal.MakeNoise();
            }
        }
        public Farm<T> GetSpecies<U>() where U:T
        {
            Farm<T> spe = new Farm<T>();
            foreach (T animal in animals)
            {
                if (animal is U)
                {
                    spe.Animals.Add(animal);
                }
            }
            return spe;
        }
        public static implicit operator Farm<Animal>(Farm<T> farm)//这样Farm<Cow>可隐式转成Farm<Animal>
        {
            Farm<Animal> result = new Farm<Animal>();
            foreach (T animal in farm)
            {
                result.Animals.Add(animal);
            }
            return result;
        }
        public static Farm<T> operator +(Farm<T> farm1, Farm<T> farm2)//T不能一个为Animal，一个为Cow，因此需定义隐式转换
        {
            Farm<T> result = new Farm<T>();
            foreach (T animal in farm1)
            {
                result.Animals.Add(animal);
            }
            foreach (T animal in farm2)
            {
                if (!result.Animals.Contains(animal))
                {
                    result.Animals.Add(animal);
                }
            }
            return result;
        }
    }
}
