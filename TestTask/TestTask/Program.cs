using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestTask
{
    class Program
    {
        static void Main(string[] args)
        {
            
            Console.ReadKey();
        }
        static void TestResult()
        {
            var t1 = new Task<Tuple<int, int>>(TaskWithResult, Tuple.Create<int, int>(8, 3));
            t1.Start();
            Console.WriteLine(t1.Result);//在访问Result时会阻塞直到任务返回
            Console.WriteLine("started");
            t1.Wait();
            Console.WriteLine("result from task: {0} {1}", t1.Result.Item1, t1.Result.Item2);
        }
        static void TestStart()
        {
            Task t1 = new TaskFactory().StartNew(TaskMethod);
            Task t2 = Task.Factory.StartNew(TaskMethod);
            Task t3 = new Task(TaskMethod);
            t3.Start();
        }
        static void TaskMethod()
        {
            Console.WriteLine("running in a task");
            Console.WriteLine("Task id: {0}", Task.CurrentId);
        }
        static Tuple<int, int> TaskWithResult(object division)
        {
            Thread.Sleep(1000);
            Tuple<int, int> div = (Tuple<int, int>)division;
            int result = div.Item1 / div.Item2;
            int reminder = div.Item1 % div.Item2;
            Console.WriteLine("task creates a result...");
            return Tuple.Create<int, int>(result, reminder);
        }
    }
}
