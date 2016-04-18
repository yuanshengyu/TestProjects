using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTimer
{
    public interface TimedDoor : Door
    {
        void DoorTimeOut(int timeOutId);
    }
}
