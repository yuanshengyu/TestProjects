using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTimer
{
    public class DoorTimerAdapter : TimerClient
    {
        private TimedDoor timedDoor;
        public DoorTimerAdapter(TimedDoor theDoor)
        {
            timedDoor = theDoor;
        }
        public virtual void TimeOut(int timeOutId)
        {
            timedDoor.DoorTimeOut(timeOutId);
        }
    }
}
