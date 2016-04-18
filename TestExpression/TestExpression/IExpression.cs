using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestExpression
{
    public interface IExpression
    {
        void Interpret(StringBuilder sb);
    }
}
