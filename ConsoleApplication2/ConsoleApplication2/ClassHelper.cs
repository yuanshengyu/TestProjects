using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace TestReflection
{
    [Help("这是类的url")]
    public class ClassHelper
    {
        [Help("这是Name对应的url")]
        public string Name { get; set; }
        [Help("这是Age对应的url")]
        public int Age { get; set; }
    }
}
