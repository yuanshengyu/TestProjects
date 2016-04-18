using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace c
{
    class Program
    {
        static void Main(string[] args)
        {
            int[] nums = new int[] { 1, 3, 5, 7, 9, 11, 13, 15 };
            bool flag = false;
            foreach (int i1 in nums)
            {
                foreach (int i2 in nums)
                {
                    foreach (int i3 in nums)
                    {
                        if (i1 + i2 + i3 == 30)
                        {
                            Console.WriteLine("{0} + {1} + {2} = 30", i1, i2, i3);
                            flag = true;
                        }
                    }
                }
            }
            if (!flag)
            {
                Console.WriteLine("无解");
            }
            Console.ReadKey();
        }
    }
}
