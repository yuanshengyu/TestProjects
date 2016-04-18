using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infecon.CSSD.Monitor.Belimed
{
    class Consts
    {
        public const string C_PIPE_HANDLE = "BelimedPipelineHandle";

        public const string C_READED_BUFFER_KEY = "ReadedBuffer";

        public const string C_SENSOR_GROUP = "SensorGroup";
    }

    /// <summary>
    /// 监控数据版本
    /// </summary>
    /// <remarks>
    /// 说明：通过 SensorDataHead.DataVer 字段来区分不同的数据版本
    ///     V0或者空白      （缺陷：SensorDataLine.AnalysedData的值不对）
    ///     V1              （修改V0版缺陷）
    /// </remarks>
    public enum DataVersion
    {
        V0,
        V1,
    }

    enum AxesName
    {
        Temperature,
        Pressure,
        AoValue,
        Other,
    }

    public enum SystemData
    {
        MessageType = 0,
        VersionOfMessageType = 1,
        Timestamp = 2,
        SystemNo = 3,
        ZoneNo = 4,
        Status = 5,
        StatusInfo = 6,
        BatchNo = 7,
        ErrorNo = 8,
        ProgramNo = 9,
        ProgramName = 10,
        ProgramPhase = 11,
        MeasurementSensor1 = 12,
        MeasurementSensor2 = 13,
        MeasurementSensor3 = 14,
        MeasurementSensor4 = 15,
        MeasurementSensor5 = 16,
        MeasurementSensor6 = 17,
        MeasurementSensor7 = 18,
        MeasurementSensor8 = 19,
        MeasurementSensor9 = 20,
        MeasurementSensor10 = 21,
        MeasurementSensor11 = 22,
        MeasurementSensor12 = 23,
        MeasurementSensor13 = 24,
        MeasurementSensor14 = 25,
        MeasurementSensor15 = 26,
        MeasurementSensor16 = 27,
        MeasurementSensor17 = 28,
        MeasurementSensor18 = 29,
        MeasurementSensor19 = 30,
        MeasurementSensor20 = 31,
        MeasurementSensor21 = 32,
        MeasurementSensor22 = 33,
        MeasurementSensor23 = 34,
        MeasurementSensor24 = 35,
        MeasurementSensor25 = 36,
        MeasurementSensor26 = 37,
        MeasurementSensor27 = 38,
        MeasurementSensor28 = 39,
        MeasurementSensor29 = 40,
        MeasurementSensor30 = 41,
        MeasurementSensor31 = 42,
        MeasurementSensor32 = 43,
        MeasurementSensor33 = 44,
        MeasurementSensor34 = 45,
        MeasurementSensor35 = 46,
        MeasurementSensor36 = 47,
        MeasurementSensor37 = 48,
        MeasurementSensor38 = 49,
        MeasurementSensor39 = 50,
        MeasurementSensor40 = 51,
        MeasurementSensor41 = 52,
        MeasurementSensor42 = 53,
        MeasurementSensor43 = 54,
        MeasurementSensor44 = 55,
        MeasurementSensor45 = 56,
        MeasurementSensor46 = 57,
        MeasurementSensor47 = 58,
        MeasurementSensor48 = 59,
        CrLf = 60,
    }
}
