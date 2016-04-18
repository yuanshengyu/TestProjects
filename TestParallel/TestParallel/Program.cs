using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace TestParallel
{
    class Program
    {
        static void Main(string[] args)
        {
            for (int i = 0; i < 16; i++)
            {
                StreamWriter sw = new StreamWriter("test.txt");
                sw.Close();
            }
            Console.ReadKey();
        }
        static void Test1()
        {
            ParallelLoopResult result =
                Parallel.For(0, 10, i =>
                {
                    Console.WriteLine("{0}, task: {1}, thread: {2}", i,
                        Task.CurrentId, Thread.CurrentThread.ManagedThreadId);
                    Thread.Sleep(10);
                });
            Console.WriteLine(result.IsCompleted);
        }
    }
}
