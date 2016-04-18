using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using Common.Logging;

namespace TestBelimed
{
    class TestQ:IJob
    {
        private static ILog log = LogManager.GetLogger(typeof(TestQ));
        public void Execute(IJobExecutionContext context)
        {
            Console.WriteLine("Hello Word!!");
        }
    }
}
