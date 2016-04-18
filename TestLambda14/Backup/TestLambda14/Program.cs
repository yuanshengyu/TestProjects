using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestLambda14
{
    delegate int TwoOperationDeletgate(int va1, int va2);
    class Program
    {
        static void PerformOperations(TwoOperationDeletgate del)
        {
            for (int i = 1; i <= 5; i++)
            {
                for (int j = 1; j <= 5; j++)
                {
                    int result = del(i, j);
                    Console.WriteLine("f({0}, {1}) = {2}", i, j, result);
                    if (j != 5) Console.WriteLine(",");
                }
                Console.WriteLine();
            }
        }

        static void Main(string[] args)
        {
        }
    }
}
