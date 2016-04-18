using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;
using System.Data;

namespace Xnlab.SQLMon
{
    public class MessageEventArgs : EventArgs
    {
        public string Message { get; set; }
        public bool Cancel { get; set; }

        public MessageEventArgs(string Message, bool Cancel)
        {
            this.Message = Message;
            this.Cancel = Cancel;
        }
    }

    public class AlertEventArgs : MessageEventArgs
    {
        public MonitorItem Item { get; set; }
        public NotifiedMonitorItem Notification { get; set; }

        public AlertEventArgs(MonitorItem Item, NotifiedMonitorItem Notification, string Message)
            : base(Message, false)
        {
            this.Item = Item;
            this.Notification = Notification;
        }
    }

    public class ServerInfoEventArgs : EventArgs
    {
        public ServerInfo Server { get; set; }
        public bool IsServer { get; set; }
        public bool Cancel { get; set; }

        public ServerInfoEventArgs()
        {
        }
    }

    public class PerformanceRecordEventArgs : EventArgs
    {
        public ServerInfo Server { get; set; }
        public PerformanceRecord Data { get; set; }

        public PerformanceRecordEventArgs()
        {
        }
    }

    public class MonitorEngine
    {
        public event EventHandler<MessageEventArgs> Message;
        public event EventHandler<AlertEventArgs> Alert;
        public event EventHandler<ServerInfoEventArgs> RequestServer;
        public event EventHandler<PerformanceRecordEventArgs> UpdateServerInfo;
        private Dictionary<string, NotifiedMonitorItem> notifiedAlerts = new Dictionary<string, NotifiedMonitorItem>();
        private PerformanceCounter cpuCounter;
        private PerformanceCounter ramCounter;
        private static MonitorEngine instance = null;
        private Timer tmrMonitorEngineRefresh = null;
        private Timer tmrPerformanceRefresh = null;
        private DataTable lastPerformanceData = null;
        private List<ServerInfoEx> userPerformanceItems = new List<ServerInfoEx>();

        public MonitorEngine()
        {
            cpuCounter = new PerformanceCounter();

            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";

            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
        }

        public static MonitorEngine Instance
        {
            get
            {
                if (instance == null)
                    instance = new MonitorEngine();
                return instance;
            }
        }

        public int CurrentCpuUsage
        {
            get { return Convert.ToInt32(cpuCounter.NextValue()); }
        }

        public int AvailableRAM
        {
            get { return Convert.ToInt32(ramCounter.NextValue()); }
        }

        public void SetMonitorInterval(string Interval)
        {
            int interval;
            if (int.TryParse(Interval, out interval) && interval > 0)
            {
                TimeSpan span = TimeSpan.FromSeconds(interval);
                tmrMonitorEngineRefresh = new Timer(new TimerCallback(OnMonitorRefreshTick), null, span, span);
            }
            else if (tmrMonitorEngineRefresh != null)
            {
                tmrMonitorEngineRefresh.Dispose();
                tmrMonitorEngineRefresh = null;
                interval = 0;
            }
            Settings.Instance.MonitorRefreshInterval = interval;
        }

        public void DisablePerformance()
        {
            SetPerformanceInterval(string.Empty);
        }

        public void SetPerformanceInterval(string Interval)
        {
            int interval;
            if (int.TryParse(Interval, out interval) && interval > 0)
            {
                TimeSpan span = TimeSpan.FromSeconds(interval);
                tmrPerformanceRefresh = new Timer(new TimerCallback(OnPerformanceRefreshTick), null, span, span);
            }
            else if (tmrPerformanceRefresh != null)
            {
                tmrPerformanceRefresh.Dispose();
                tmrPerformanceRefresh = null;
                interval = 0;
            }
            Settings.Instance.PerformanceInterval = interval;
        }

        private void OnMonitorRefreshTick(object sender)
        {
            CheckMonitorItems();
        }

        private void OnPerformanceRefreshTick(object sender)
        {
            CheckPerformance();
        }

        private bool IsNotified(MonitorItem Item, string Key, string RuntimeValue, string Message, out NotifiedMonitorItem Notified)
        {
            Notified = null;
            var key = Item.ToString() + ", " + Key + ", " + RuntimeValue;
            if (!notifiedAlerts.ContainsKey(key))
            {
                Notified = new NotifiedMonitorItem { Server = Item.Server, CurrentValue = Message, CreatedDate = DateTime.Now };
                notifiedAlerts.Add(key, Notified);
                if (Settings.Instance.LogHistory)
                    Settings.Instance.NotifiedAlerts.Add(Notified);
                return false;
            }
            else
                return true;
        }

        private bool IsTextMatch(string Content, string Pattern)
        {
            try
            {
                if (Content.IndexOf(Pattern, StringComparison.InvariantCultureIgnoreCase) != -1)
                    return true;
                else if (Regex.IsMatch(Content, Pattern))
                    return true;
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void OnUpdateServerInfo(PerformanceRecord Record, ServerInfo Server, bool IsServer)
        {
            UpdateServerInfo(this, new PerformanceRecordEventArgs() { Data = Record, Server = Server });
            if (Settings.Instance.LogHistory)
            {
                var record = new HistoryRecord(Record)
                {
                    Date = DateTime.Now.ToString(),
                    Key = History.GetKey(Server, IsServer)
                };
                History.AddRecords(new List<HistoryRecord> { record });
            }
        }

        public void RemoveUserPerformanceItem(ServerInfo Server, bool IsServer)
        {
            var item = userPerformanceItems.FirstOrDefault(p => p.Server == Server.Server
                && p.Database == Server.Database && p.IsServer == IsServer);
            if (item != null)
                userPerformanceItems.Remove(item);
        }

        public void AddUserPerformanceItem(ServerInfo Server, bool IsServer)
        {
            if (!userPerformanceItems.Exists(p => p.Server == Server.Server
                && p.Database == Server.Database && p.IsServer == IsServer))
                userPerformanceItems.Add(new ServerInfoEx
                {
                    AuthType = Server.AuthType,
                    Database = Server.Database,
                    IsEncrypted = Server.IsEncrypted,
                    IsServer = IsServer,
                    Password = Server.Password,
                    Server = Server.Server,
                    User = Server.User
                });
        }

        public void CheckPerformance()
        {
            try
            {
                if (RequestServer != null)
                {
                    var e = new ServerInfoEventArgs();
                    RequestServer(this, e);
                    if (!e.Cancel)
                    {
                        MonitorEngine.Instance.AddUserPerformanceItem(e.Server, e.IsServer);
                        CheckPerformanceItem(e.Server, e.IsServer);
                    }
                }

                if (this.Equals(MonitorEngine.Instance))
                {
                    Settings.Instance.PerformanceItems.ForEach(i =>
                        {
                            var exists = MonitorEngine.Instance.userPerformanceItems.Exists(p => p.Server == i.Server
                                && p.Database == i.Database && p.IsServer == i.IsServer);
                            if (!exists)
                                CheckPerformanceItem(i, i.IsServer);
                        });
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private void CheckPerformanceItem(ServerInfo Server, bool IsServer)
        {
            PerformanceRecord record;
            if (IsServer)
            {
                var sql = @"declare @now 		datetime
declare @cpu_busy 	bigint
declare @io_busy	bigint
declare @idle		bigint
declare @pack_received	bigint
declare @pack_sent	bigint
declare @pack_errors	bigint
declare @connections	bigint
declare @total_read	bigint
declare @total_write	bigint
declare @total_errors	bigint

declare @oldcpu_busy 	bigint	/* used to see if DataServer has been rebooted */
declare @interval	bigint
declare @mspertick	bigint	/* milliseconds per tick */


/*
**  Set @mspertick.  This is just used to make the numbers easier to handle
**  and avoid overflow.
*/
select @mspertick = convert(int, @@timeticks / 1000.0)

/*
**  Get current monitor values.
*/
select
	@now = getdate(),
	@cpu_busy = @@cpu_busy,
	@io_busy = @@io_busy,
	@idle = @@idle,
	@pack_received = @@pack_received,
	@pack_sent = @@pack_sent,
	@connections = @@connections,
	@pack_errors = @@packet_errors,
	@total_read = @@total_read,
	@total_write = @@total_write,
	@total_errors = @@total_errors

/*
**  Check to see if DataServer has been rebooted.  If it has then the
**  value of @@cpu_busy will be less than the value of spt_monitor.cpu_busy.
**  If it has update spt_monitor.
*/
select @oldcpu_busy = cpu_busy
	from master.dbo.spt_monitor
if @oldcpu_busy > @cpu_busy
begin
	update master.dbo.spt_monitor
		set
			lastrun = @now,
			cpu_busy = @cpu_busy,
			io_busy = @io_busy,
			idle = @idle,
			pack_received = @pack_received,
			pack_sent = @pack_sent,
			connections = @connections,
			pack_errors = @pack_errors,
			total_read = @total_read,
			total_write = @total_write,
			total_errors = @total_errors
end

/*
**  Now print out old and new monitor values.
*/
set nocount on
select @interval = datediff(ss, lastrun, @now)
	from master.dbo.spt_monitor
/* To prevent a divide by zero error when run for the first
** time after boot up
*/
if @interval = 0
	select @interval = 1
select last_run = lastrun, current_run = @now, seconds = @interval,
	cpu_busy_total = convert(bigint, (@cpu_busy / 1000.0 * @mspertick)),
	cpu_busy_current = convert(bigint, ((@cpu_busy - cpu_busy)
		 / 1000.0 * @mspertick)),
	cpu_busy_percentage = convert(bigint, (((@cpu_busy - cpu_busy)
		 / 1000.0 * @mspertick) / @interval * 100.0)),
	io_busy_total = convert(bigint, (@io_busy / 1000 * @mspertick)),
	io_busy_current = convert(bigint, ((@io_busy - io_busy)
		 / 1000.0 * @mspertick)),
	io_busy_percentage = convert(bigint, (((@io_busy - io_busy)
		 / 1000.0 * @mspertick) / @interval * 100.0)),
	idle_total = convert(bigint, (convert(bigint,@idle) / 1000.0 * @mspertick)),
	idle_current = convert(bigint, ((@idle - idle)
		 / 1000.0 * @mspertick)),
	idle_percentage = convert(bigint, (((@idle - idle)
		 / 1000.0 * @mspertick) / @interval * 100.0)),
	packets_received_total = @pack_received,
	packets_received_current = @pack_received - pack_received,
	packets_sent_total = @pack_sent,
	packets_sent_current = @pack_sent - pack_sent,
	packet_errors_total = @pack_errors,
	packet_errors_current = @pack_errors - pack_errors,
	total_read = @total_read,
	current_read = @total_read - total_read,
	total_write = @total_write,
	current_write =	@total_write - total_write,
	total_errors = @total_errors,
	current_errors = @total_errors - total_errors,
	connections_total = @connections,
	connections_current = @connections - connections
from master.dbo.spt_monitor

/*
**  Now update spt_monitor
*/
update master.dbo.spt_monitor
	set
		lastrun = @now,
		cpu_busy = @cpu_busy,
		io_busy = @io_busy,
		idle = @idle,
		pack_received = @pack_received,
		pack_sent = @pack_sent,
		connections = @connections,
		pack_errors = @pack_errors,
		total_read = @total_read,
		total_write = @total_write,
		total_errors = @total_errors";
                var serverInfo = SQLHelper.Query(sql, Server);
                var row = serverInfo.Rows[0];
                var cpu_busy_current = Convert.ToInt64(row["cpu_busy_current"]);
                var io_busy_current = Convert.ToInt64(row["io_busy_current"]);
                var current_read = Convert.ToInt64(row["current_read"]);
                var current_write = Convert.ToInt64(row["current_write"]);
                var packets_received_current = Convert.ToInt64(row["packets_received_current"]);
                var packets_sent_current = Convert.ToInt64(row["packets_sent_current"]);
                var connections_current = Convert.ToInt64(row["connections_current"]);
                record = new PerformanceRecord
                {
                    Value1 = Convert.ToInt64(row["cpu_busy_current"]),
                    Value2 = Convert.ToInt64(row["io_busy_current"]),
                    Value3 = Convert.ToInt64(row["current_read"]),
                    Value4 = Convert.ToInt64(row["current_write"]),
                    Value5 = Convert.ToInt64(row["packets_received_current"]),
                    Value6 = Convert.ToInt64(row["packets_sent_current"]),
                    Value7 = Convert.ToInt64(row["connections_current"]),
                    Value8 = Convert.ToInt64(row["io_busy_total"]),
                    Value9 = Convert.ToInt64(row["cpu_busy_total"]),
                    Value10 = Convert.ToInt64(row["total_read"]),
                    Value11 = Convert.ToInt64(row["total_write"]),
                    Value12 = Convert.ToInt64(row["packets_received_total"]),
                    Value13 = Convert.ToInt64(row["packets_sent_total"]),
                    Value14 = Convert.ToInt64(row["connections_total"]),
                };
                OnUpdateServerInfo(record, Server, IsServer);
            }
            else
            {
                var data = QueryEngine.GetDatabaseIOInfo(Server);
                if (lastPerformanceData != null)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        var row = data.Rows[i];
                        var last = lastPerformanceData.Rows[i];
                        row["CurrentNumberReads"] = Convert.ToInt64(row["NumberReads"]) - Convert.ToInt64(last["NumberReads"]);
                        row["CurrentNumberWrites"] = Convert.ToInt64(row["NumberWrites"]) - Convert.ToInt64(last["NumberWrites"]);
                    }
                }
                var db = data.Rows[0];
                record = new PerformanceRecord
                {
                    Value1 = Convert.ToInt64(db["NumberReads"]),
                    Value2 = Convert.ToInt64(db["BytesRead"]),
                    Value3 = Convert.ToInt64(db["NumberWrites"]),
                    Value4 = Convert.ToInt64(db["BytesWritten"]),
                    Value5 = Convert.ToInt64(db["CurrentNumberReads"]),
                    Value6 = Convert.ToInt64(db["CurrentNumberWrites"]),
                    Value13 = Convert.ToInt64(db["IsStall"]),
                    Value16 = Convert.ToDateTime(db["StartDate"]),
                    Value15 = Convert.ToInt64(db["FileCount"])
                };
                bool hasLog = data.Rows.Count > 1;
                if (hasLog)
                {
                    var log = data.Rows[1];
                    record.Value7 = Convert.ToInt64(log["NumberReads"]);
                    record.Value8 = Convert.ToInt64(log["BytesRead"]);
                    record.Value9 = Convert.ToInt64(log["NumberWrites"]);
                    record.Value10 = Convert.ToInt64(log["BytesWritten"]);
                    record.Value11 = Convert.ToInt64(log["CurrentNumberReads"]);
                    record.Value12 = Convert.ToInt64(log["CurrentNumberWrites"]);
                    record.Value14 = Convert.ToInt64(log["IsStall"]);
                    record.Value15 += Convert.ToInt64(log["FileCount"]);
                }
                OnUpdateServerInfo(record, Server, IsServer);
                lastPerformanceData = data;
            }
        }

        private void OnError(Exception ex)
        {
            if (Message != null)
                Message(this, new MessageEventArgs(ex.Message, ex is System.Data.SqlClient.SqlException));
        }

        private void CheckMonitorItems()
        {
            try
            {
                Settings.Instance.MonitorItems.Where(i => i.IsEnabled).ForEach(item =>
                    {
                        var server = Settings.Instance.FindServer(item.Server);
                        if (server != null)
                        {
                            server.Database = "master";
                            switch (item.AlertType)
                            {
                                case AlertTypes.SQL:
                                    switch (item.CondictionType)
                                    {
                                        case 0:
                                        case 1:
                                            var sessions = SQLHelper.Query(QueryEngine.SQLProcesses, server);
                                            sessions.Rows.Cast<DataRow>().ForEach(r =>
                                                {
                                                    var id = r["spid"].ToString();
                                                    var sql = QueryEngine.GetSessionSQL(id, server);
                                                    if (IsTextMatch(sql, item.Target))
                                                    {
                                                        bool result;
                                                        if (item.CondictionType == 1)
                                                        {
                                                            TimeSpan span = ((DateTime)r["last_batch_end"]).Subtract((DateTime)r["last_batch_begin"]);
                                                            result = span > TimeSpan.FromSeconds(item.CondictionValue);
                                                            sql = span.ToString() + ", " + sql;
                                                        }
                                                        else
                                                            result = true;
                                                        if (result)
                                                        {
                                                            var key = id + "|" + r["hostname"].ToString() + "|" + r["hostprocess"].ToString();
                                                            ShowAlert(item, key, sql);
                                                        }
                                                    }
                                                });
                                            break;
                                        case 2:
                                            var tasks = SQLHelper.Query(QueryEngine.SQLWaitingTasks, server);
                                            tasks.Rows.Cast<DataRow>().ForEach(r =>
                                                {
                                                    var id = r["[Blocking Session Id]"].ToString();
                                                    if (!string.IsNullOrEmpty(id))
                                                    {
                                                        var sql = QueryEngine.GetSessionSQL(id, server);
                                                        if (string.IsNullOrEmpty(sql))
                                                            sql = "session id = " + id;
                                                        ShowAlert(item, sql, sql);
                                                    }
                                                });
                                            break;
                                        default:
                                            break;
                                    }
                                    break;
                                case AlertTypes.Server:
                                    switch (item.CondictionType)
                                    {
                                        case 0:
                                            try
                                            {
                                                SQLHelper.ExecuteScalar("SELECT @@version", server);
                                            }
                                            catch (Exception ex)
                                            {
                                                ShowAlert(item, server.Server, ex.Message);
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                    break;
                                //case AlertTypes.CPU:
                                //    var cpu = CurrentCpuUsage;
                                //    if (cpu > item.CondictionValue)
                                //        ShowAlert(item, cpu.ToString());
                                //    break;
                                //case AlertTypes.Memory:
                                //    var mem = AvailableRAM;
                                //    if (mem > item.CondictionValue)
                                //        ShowAlert(item, mem.ToString());
                                //    break;
                                //case AlertTypes.Diskspace:
                                //    break;
                                default:
                                    break;
                            }
                        }
                    });

            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private void ShowAlert(MonitorItem Item, string Key, string CurrentValue)
        {
            string message = Settings.Instance.AlertTemplate;
            if (string.IsNullOrEmpty(message))
                message = Settings.DefaultTemplate;
            string type = string.Empty;
            string action = string.Empty;
            switch (Item.AlertType)
            {
                case AlertTypes.SQL:
                    action = CurrentValue;
                    switch (Item.CondictionType)
                    {
                        case 0:
                            type = "SQL executes: ";
                            break;
                        case 1:
                            type = "SQL lasts: ";
                            break;
                        case 2:
                            type = "SQL blocked: ";
                            break;
                        default:
                            break;
                    }
                    break;
                case AlertTypes.Server:
                    break;
                //case AlertTypes.CPU:
                //    message = string.Format("CPU usage is now {0}, higher than {1}", CurrentValue, Item.CondictionValue);
                //    break;
                //case AlertTypes.Memory:
                //    message = string.Format("Available memory is now {0}, lower than {1}", CurrentValue, Item.CondictionValue);
                //    break;
                //case AlertTypes.Diskspace:
                //    break;
                default:
                    break;
            }
            if (!string.IsNullOrEmpty(message))
            {
                var fullMessage = message.Replace("#Type#", type);
                fullMessage = fullMessage.Replace("#Action#", action);
                fullMessage = fullMessage.Replace("#Server#", Item.Server);
                fullMessage = fullMessage.Replace("#Now#", DateTime.Now.ToString());
                NotifiedMonitorItem notified;
                if (!IsNotified(Item, Key, type + action, fullMessage, out notified))
                    if (Alert != null)
                        Alert(this, new AlertEventArgs(Item, notified, fullMessage));
            }
        }
    }
}
