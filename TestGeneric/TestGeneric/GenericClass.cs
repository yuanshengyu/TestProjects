using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestGeneric
{
    public class GenericClass<T1,T2>
    {
        public string GetString()
        {
            return typeof(T1).ToString() + "   " + typeof(T2).ToString();
        }
    }
}
