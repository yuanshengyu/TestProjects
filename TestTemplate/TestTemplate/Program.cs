using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTemplate
{
    class Program
    {
        static void Main(string[] args)
        {
            Report report = new Report();
            report.CreateNewDocument(@"E:\MyProjects\TestTemplate\TestTemplate\模板1.docx");
            report.InsertValue("Mark2", "世界杯");
            report.SaveDocument(@"E:\MyProjects\TestTemplate\TestTemplate\Report.doc");
        }
    }
}
