using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Xnlab.Filio;

namespace Xnlab.SQLMon
{
    public class PerformanceRecord
    {
        public long Value1 { get; set; }
        public long Value2 { get; set; }
        public long Value3 { get; set; }
        public long Value4 { get; set; }
        public long Value5 { get; set; }
        public long Value6 { get; set; }
        public long Value7 { get; set; }
        public long Value8 { get; set; }
        public long Value9 { get; set; }
        public long Value10 { get; set; }
        public long Value11 { get; set; }
        public long Value12 { get; set; }
        public long Value13 { get; set; }
        public long Value14 { get; set; }
        public long Value15 { get; set; }
        public DateTime Value16 { get; set; }
    }

    internal class HistoryRecord : PerformanceRecord, ICustomBinarySerializable
    {
        public string Date { get; set; }
        public string Key { get; set; }

        internal HistoryRecord(PerformanceRecord Record)
        {
            //damn, it's crazy! I wanted to use reflection...
            Value1 = Record.Value1;
            Value2 = Record.Value2;
            Value3 = Record.Value3;
            Value4 = Record.Value4;
            Value5 = Record.Value5;
            Value6 = Record.Value6;
            Value7 = Record.Value7;
            Value8 = Record.Value8;
            Value9 = Record.Value9;
            Value10 = Record.Value10;
            Value11 = Record.Value11;
            Value12 = Record.Value12;
            Value13 = Record.Value13;
            Value14 = Record.Value14;
        }

        public void WriteDataTo(BinaryWriter writer)
        {
            writer.Write(Date);
            writer.Write(Key);
            writer.Write(Value1);
            writer.Write(Value2);
            writer.Write(Value3);
            writer.Write(Value4);
            writer.Write(Value5);
            writer.Write(Value6);
            writer.Write(Value7);
            writer.Write(Value8);
            writer.Write(Value9);
            writer.Write(Value10);
            writer.Write(Value11);
            writer.Write(Value12);
            writer.Write(Value13);
            writer.Write(Value14);
        }

        public void SetDataFrom(BinaryReader reader, bool Full)
        {
            Date = reader.ReadString();
            Key = reader.ReadString();
            Value1 = reader.ReadInt64();
            Value2 = reader.ReadInt64();
            Value3 = reader.ReadInt64();
            Value4 = reader.ReadInt64();
            Value5 = reader.ReadInt64();
            Value6 = reader.ReadInt64();
            Value7 = reader.ReadInt64();
            Value8 = reader.ReadInt64();
            Value9 = reader.ReadInt64();
            Value10 = reader.ReadInt64();
            Value11 = reader.ReadInt64();
            Value12 = reader.ReadInt64();
            Value13 = reader.ReadInt64();
            Value14 = reader.ReadInt64();
        }
    }

    internal class HistoryDate : ICustomBinarySerializable
    {
        public string Date { get; set; }
        public long Index { get; set; }

        public void WriteDataTo(BinaryWriter writer)
        {
            writer.Write(Date);
            writer.Write(Index);
        }

        public void SetDataFrom(BinaryReader reader, bool Full)
        {
            Date = reader.ReadString();
            Index = reader.ReadInt64();
        }
    }

    internal enum DateTypes
    {
        Hour = 0,
        Day = 1,
        Week = 2,
        Month = 3,
        Year = 4
    }

    internal class History
    {
        internal const string Separator = "|";
        private const string indexExtension = ".idx";
        private const string dataExtension = ".dat";
        private const string historyFile = "HistoryData";
        private const string datesFile = "HistoryDates";
        private static readonly object _syncRoot = new object();

        internal static string GetKey(ServerInfo Server, bool IsServer)
        {
            return (Server.Server + (IsServer ? string.Empty : "." + Server.Database)).ToLower();
        }

        private static string GetFile(bool IsHistory, bool IsIndex)
        {
            return (IsHistory ? historyFile : datesFile) + (IsIndex ? indexExtension : dataExtension);
        }

        internal static void AddRecords(List<HistoryRecord> Records)
        {
            lock (_syncRoot)
            {
                using (FileStream dataIndexStream = new FileStream(GetFile(true, true), FileMode.OpenOrCreate))
                {
                    using (FileStream dataContentStream = new FileStream(GetFile(true, false), FileMode.OpenOrCreate))
                    {
                        var historyFormatter = new CustomBinaryFormatter(dataIndexStream, dataContentStream);
                        historyFormatter.Register<HistoryRecord>(1);

                        historyFormatter.MoveToEnd();
                        var historyCount = historyFormatter.Count;
                        Records.ForEach(r =>
                        {
                            historyFormatter.Serialize<HistoryRecord>(r);
                        });

                        historyFormatter.Flush();

                        using (FileStream dateIndexStream = new FileStream(GetFile(false, true), FileMode.OpenOrCreate))
                        {
                            using (FileStream dateContentStream = new FileStream(GetFile(false, false), FileMode.OpenOrCreate))
                            {
                                var dateFormatter = new CustomBinaryFormatter(dateIndexStream, dateContentStream);
                                dateFormatter.Register<HistoryDate>(1);

                                var dateCount = dateFormatter.Count;
                                var today = DateTime.Now.Date.AddDays(1);
                                var yesterday = DateTime.Now.Date;
                                long todayIndex = -1;
                                var foundYesterday = false;
                                for (long i = 0; i < dateCount; i++)
                                {
                                    var date = dateFormatter.Deserialize<HistoryDate>(false);
                                    if (DateTime.Parse(date.Date) == today)
                                        todayIndex = i;
                                    if (DateTime.Parse(date.Date) == yesterday)
                                        foundYesterday = true;
                                }
                                if (!foundYesterday)
                                {
                                    dateFormatter.MoveToEnd();
                                    dateFormatter.Serialize<HistoryDate>(new HistoryDate { Date = yesterday.ToString(), Index = historyCount });
                                }

                                if (todayIndex != -1)
                                {
                                    dateFormatter.MoveTo(todayIndex);
                                    dateFormatter.Serialize<HistoryDate>(new HistoryDate { Date = today.ToString(), Index = historyFormatter.Count }, true);
                                }
                                else
                                {
                                    dateFormatter.MoveToEnd();
                                    dateFormatter.Serialize<HistoryDate>(new HistoryDate { Date = today.ToString(), Index = historyFormatter.Count });
                                }

                                dateFormatter.Flush();
                            }
                        }
                    }
                }
            }
        }

        internal static List<HistoryRecord> GetRecords(ServerInfo Server, bool IsServer, DateTypes DateType, DateTime StartDate)
        {
            var records = new List<HistoryRecord>();
            var key = GetKey(Server, IsServer);
            using (FileStream dateIndexStream = new FileStream(GetFile(false, true), FileMode.OpenOrCreate))
            {
                using (FileStream dateContentStream = new FileStream(GetFile(false, false), FileMode.OpenOrCreate))
                {
                    var dateFormatter = new CustomBinaryFormatter(dateIndexStream, dateContentStream);
                    dateFormatter.Register<HistoryDate>(1);

                    var endDate = DateTime.Now.Date;
                    var samplingSpan = 1;
                    switch (DateType)
                    {
                        case DateTypes.Hour:
                            endDate = StartDate.AddDays(1);
                            samplingSpan = 1;
                            break;
                        case DateTypes.Day:
                            endDate = StartDate.AddDays(1);
                            samplingSpan = 24;
                            break;
                        case DateTypes.Week:
                            endDate = StartDate.AddDays(7);
                            samplingSpan = 7 * 24;
                            break;
                        case DateTypes.Month:
                            endDate = StartDate.AddMonths(1);
                            samplingSpan = 31 * 24;
                            break;
                        case DateTypes.Year:
                            endDate = StartDate.AddYears(1);
                            samplingSpan = 365 * 24;
                            break;
                        default:
                            break;
                    }
                    var count = dateFormatter.Count;
                    System.Diagnostics.Debug.WriteLine("all date count:" + count);
                    System.Diagnostics.Debug.WriteLine("start date:" + StartDate);
                    var dates = new List<HistoryDate>();
                    for (long i = 0; i < count; i++)
                    {
                        var date = dateFormatter.Deserialize<HistoryDate>(false);
                        var dateTime = DateTime.Parse(date.Date);
                        System.Diagnostics.Debug.WriteLine("current date:" + dateTime);
                        if (StartDate.Date <= dateTime && dateTime <= endDate)
                            dates.Add(date);
                    }
                    System.Diagnostics.Debug.WriteLine("valid date count:" + dates.Count);
                    if (dates.Count > 0)
                    {
                        var start = dates.Aggregate((d1, d2) => DateTime.Parse(d1.Date) < DateTime.Parse(d2.Date) ? d1 : d2);
                        var end = dates.Aggregate((d1, d2) => DateTime.Parse(d1.Date) > DateTime.Parse(d2.Date) ? d1 : d2);

                        using (FileStream dataIndexStream = new FileStream(GetFile(true, true), FileMode.OpenOrCreate))
                        {
                            using (FileStream dataContentStream = new FileStream(GetFile(true, false), FileMode.OpenOrCreate))
                            {
                                var historyFormatter = new CustomBinaryFormatter(dataIndexStream, dataContentStream);
                                historyFormatter.Register<HistoryRecord>(1);

                                for (long i = start.Index; i < end.Index; i += samplingSpan)
                                {
                                    historyFormatter.MoveTo(i);
                                    var record = historyFormatter.Deserialize<HistoryRecord>(false);
                                    if (record.Key == key)
                                        records.Add(record);
                                }
                                historyFormatter.Close();
                                dataContentStream.Close();
                            }
                            dataIndexStream.Close();
                        }
                    }
                    dateFormatter.Close();
                    dateContentStream.Close();
                }
                dateIndexStream.Close();
            }
            return records;
        }
    }
}