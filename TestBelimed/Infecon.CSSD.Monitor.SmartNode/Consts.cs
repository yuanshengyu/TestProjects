using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infecon.CSSD.Monitor.SmartNode
{
    class Consts
    {
        public const string C_TCP_LISTENER_KEY = "TcpListener";

        public const string C_READED_BUFFER_KEY = "ReadedBuffer";

        public const string C_SENSOR_GROUP_KEY = "SensorGroup";

    }

    enum SensorModel
    {
        // 温湿
        SD1HT = 11,

        // 温湿
        TH80 = 12,

        // BlackBox，温湿
        BlackBox = 20,
        WTHS = 21,
    }

}
