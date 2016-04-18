using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.IO;

namespace TestDeepCopy
{
    class Program
    {
        static void Main(string[] args)
        {
            Student s1 = new Student { Name = "Jack", Age = 16 };
            Student s2 = DeepCopy<Student>(s1);
            s2.Name = "John";
            s2.Age = 17;
            Console.WriteLine(s1.ToString());
            Console.WriteLine(s2.ToString());
            Console.ReadKey();
        }
        static T DeepCopy<T>(T obj)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
                stream.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }
    }
    [Serializable]
    class Student
    {
        public string Name;
        public int Age;
        public override string ToString()
        {
            return string.Format("Name = {0}, Age = {1}", Name, Age);
        }
    }
}
