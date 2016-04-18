using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTimer
{
    public interface Door
    {
        void Lock();
        void Unlock();
        bool IsDoorOpen();
    }
}
