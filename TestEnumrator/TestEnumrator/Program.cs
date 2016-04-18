using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestEnumrator
{
    class Program
    {
        static void Main(string[] args)
        {
            Friends friends = new Friends();
            foreach (Friend f in friends)
            {
                Console.WriteLine(f.Name);
            }
            Console.ReadLine();
        }
    }
}
