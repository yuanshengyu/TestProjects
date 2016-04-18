using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestDelegate1
{
    public delegate int TakesAwhileDelegate(int data, int ms);
    class Program
    {
        static void Main(string[] args)
        {
            TestCallback();
            Console.ReadKey();
        }
        static void TestCallback()
        {
            TakesAwhileDelegate d = TakesAWhile;
            IAsyncResult ar = d.BeginInvoke(1, 3000, funccallback, d);
            //若在委托结束之前不等待委托完成其任务就结束主线程，委托线程就会停止。
            for(int i=0;i<20;i++)
            {
                Console.Write(".");
                Thread.Sleep(100);
            }
        }
        static void TestWaitOne()
        {
            TakesAwhileDelegate d = TakesAWhile;
            IAsyncResult ar = d.BeginInvoke(1, 3000, null, null);
            //若在委托结束之前不等待委托完成其任务就结束主线程，委托线程就会停止。
            while (true)
            {
                Console.Write(".");
                if (ar.AsyncWaitHandle.WaitOne(100, false))
                {
                    Console.WriteLine("Can get the result now");
                    break;
                }
                Thread.Sleep(100);
            }
            int result = d.EndInvoke(ar);
            Console.WriteLine("result: {0}", result);
        }
        static void TestIsCompleted()
        {
            TakesAwhileDelegate d = TakesAWhile;
            IAsyncResult ar = d.BeginInvoke(1, 3000, null, null);
            while (!ar.IsCompleted)//若在委托结束之前不等待委托完成其任务就结束主线程，委托线程就会停止。
            {
                Console.Write(".");
                Thread.Sleep(100);
            }
            int result = d.EndInvoke(ar);
            Console.WriteLine("result: {0}", result);
        }
        static int TakesAWhile(int data, int ms)
        {
            Console.WriteLine("TakesAWhile started");
            Thread.Sleep(ms);
            Console.WriteLine("TakesAWhile completed");
            return ++data;
        }
        static void funccallback(IAsyncResult ar)
        {
            TakesAwhileDelegate d = ar.AsyncState as TakesAwhileDelegate;
            int result = d.EndInvoke(ar);
            Console.WriteLine("result: {0}", result);
        }
    }
}
