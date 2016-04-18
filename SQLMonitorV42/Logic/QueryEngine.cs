using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

namespace Xnlab.SQLMon
{
    internal class QueryEngine
    {
        internal const int DBStallThreshold = 20;
        internal const string Dot = ".";

        internal const string DefaultSchema = "dbo";
        internal const string SQLWaitingTasks = "SELECT session_id AS [Session Id], exec_context_id AS [Exec Context Id], wait_duration_ms AS [Wait Duration ms], wait_type AS [Wait Type], blocking_session_id AS [Blocking Session Id], blocking_exec_context_id AS [Blocking Exec Context Id], resource_description AS [Resource Description] FROM sys.dm_os_waiting_tasks WITH (NOLOCK)";
        internal const string SQLProcesses = "SELECT s.session_id AS spid, s.login_time, s.host_name AS hostname, s.host_process_id AS hostprocess, s.login_name AS loginname, CASE WHEN (s.reads + s.writes) = 0 AND r.reads IS NOT NULL THEN (r.reads + r.writes) ELSE (s.reads + s.writes) END AS physical_io, CASE WHEN s.cpu_time = 0 AND r.cpu_time IS NOT NULL THEN r.cpu_time ELSE s.cpu_time END AS cpu, s.program_name, DB_NAME(r.database_id) AS db, s.last_request_start_time AS last_batch_begin, CASE WHEN s.status = 'running' THEN GETDATE() ELSE dateadd(ms, s.cpu_time, s.last_request_end_time) END AS last_batch_end, s.status, CASE WHEN r.blocking_session_id <> 0 THEN -1 ELSE (CASE WHEN s.status = 'running' THEN 1 ELSE 0 END) END AS enabled, CASE WHEN r.percent_complete IS NULL THEN 0 ELSE r.percent_complete END AS percent_complete, r.blocking_session_id FROM sys.dm_exec_sessions s WITH (NOLOCK) LEFT JOIN sys.dm_exec_requests r WITH (NOLOCK) ON s.session_id = r.session_id WHERE s.is_user_process = 1";
        internal const string SQLJobs = "SELECT j.job_id AS spid, j.name AS program_name, CAST(a.last_executed_step_id AS nvarchar(10)) AS dbid, 0 AS cpu, 0 AS physical_io, NULL AS login_time, a.start_execution_date AS last_batch_begin, a.stop_execution_date AS last_batch_end, CASE Enabled WHEN 1 THEN 'Enabled' ELSE 'Disabled' END AS status, @@SERVICENAME AS hostname, @@SPID AS hostprocess, NULL AS cmd, SYSTEM_USER AS loginname, enabled, 0 AS percent_complete FROM msdb.dbo.sysjobs j WITH (NOLOCK) LEFT JOIN msdb.dbo.sysjobactivity a WITH (NOLOCK) on j.job_id = a.job_id WHERE (a.session_id = (SELECT max(session_id) FROM msdb.dbo.sysjobactivity WITH (NOLOCK) WHERE job_id = j.job_id AND start_execution_date IS NOT NULL)) ORDER BY program_name";
        internal const string SQLLockedObjects = @"SELECT l.request_session_id AS SPID, s.program_name AS ProgramName, DB_NAME(l.resource_database_id) AS DatabaseName, schema_name(o.schema_id) AS SchemaName, o.name AS ObjectName FROM master.sys.dm_tran_locks l
LEFT JOIN sys.all_objects o ON o.object_id = l.resource_associated_entity_id
LEFT JOIN sys.dm_exec_sessions s ON l.request_session_id = s.session_id
WHERE l.resource_associated_entity_id <> 0 AND o.name IS NOT NULL";
        //t.parent_task_address
        private const string SQLSessions = @"SELECT e.session_id, DB_NAME(r.database_id) AS database_name, ISNULL(r.status, e.status) AS session_status, e.transaction_isolation_level, ISNULL(r.open_transaction_count, 0) AS open_transaction_count, r.command, ISNULL(r.blocking_session_id, 0) AS blocking_session_id, ISNULL(r.sql_handle, c.most_recent_sql_handle) as sql_handle, 
q.text, SUBSTRING(q.text, (r.statement_start_offset/2)+1, ((CASE r.statement_end_offset WHEN -1 THEN DATALENGTH(q.text) ELSE r.statement_end_offset END - r.statement_start_offset)/2)+1) as current_statement, ISNULL(ISNULL(r.task_address, t.task_address), 0) AS task_address, 0 AS parent_task_address, ISNULL(t.worker_address, 0) AS worker_address, ISNULL(w.scheduler_address, 0) AS scheduler_address, w.status as worker_status, ISNULL(w.is_sick, 0) AS is_sick, w.is_in_cc_exception, ISNULL(w.is_fatal_exception, 0) AS is_fatal_exception, ISNULL(w.state, '') as worker_state, ISNULL(w.tasks_processed_count, 0) AS tasks_processed_count, ISNULL(w.exception_num, 0) AS exception_num, ISNULL(w.return_code, 0) AS return_code, ISNULL(t.task_state, '') AS task_state, ISNULL(s.scheduler_id, 0) AS scheduler_id, ISNULL(s.current_workers_count, 0) AS current_workers_count, ISNULL(s.active_workers_count, 0) AS active_workers_count, ISNULL(s.is_idle, 0) AS is_idle
FROM sys.dm_exec_sessions e
LEFT JOIN sys.dm_exec_requests r ON e.session_id = r.session_id
LEFT JOIN sys.dm_os_tasks t ON e.session_id = t.session_id
LEFT JOIN sys.dm_os_workers w ON t.worker_address = w.worker_address
LEFT JOIN sys.dm_os_schedulers s ON w.scheduler_address = s.scheduler_address
LEFT JOIN sys.dm_exec_connections c ON c.session_id = e.session_id
CROSS APPLY sys.dm_exec_sql_text(ISNULL(r.sql_handle, c.most_recent_sql_handle)) as q
ORDER BY e.session_id";
        //http://thesqlguy.wordpress.com/2010/11/15/sql-2005-blocking-chains-a-friendly-display-using-cte-and-recursion/
        private const string SQLLockedProcesses = @"DECLARE @processes TABLE(
	SPID SMALLINT,
	BlockingSPID SMALLINT,
	Definition VARCHAR(MAX)
)

INSERT INTO @processes
SELECT
	s.spid, 
	BlockingSPID = s.blocked, 
	Definition = CAST(text AS VARCHAR(MAX))
FROM sys.sysprocesses s WITH (nolock)
	CROSS APPLY sys.dm_exec_sql_text (sql_handle)
--WHERE s.spid > 50;

;WITH Blocking(SPID, BlockingSPID, BlockingStatement, RowNo, LevelRow)
AS
(
	SELECT
		s.SPID, 
		s.BlockingSPID, 
		s.Definition, 
		ROW_NUMBER() OVER(ORDER BY s.SPID) AS RowNo,
		0 AS LevelRow
	FROM @processes s
		INNER JOIN @processes s1 ON s.SPID = s1.BlockingSPID
	WHERE s.BlockingSPID = 0
		UNION ALL
	SELECT
		r.SPID,
		r.BlockingSPID, 
		r.Definition,
		d.RowNo,
		d.LevelRow + 1
	FROM @processes r
		INNER JOIN Blocking d ON r.BlockingSPID = d.SPID
	WHERE r.BlockingSPID > 0
)
SELECT 
	BlockingSPID, 
	SPID, 
	BlockingStatement, 
	MIN(RowNo) AS LockGroup, 
	LevelRow 
FROM Blocking
GROUP BY BlockingSPID, SPID, BlockingStatement, LevelRow
ORDER BY MIN(RowNo), LevelRow";

        internal const string SQLObjectScripts = @"Select s.name, s.create_date AS CreateDate, s.modify_date AS ModifyDate, s.type, c.text from syscomments c WITH (NOLOCK) left join sys.objects s WITH (NOLOCK) on c.id = s.object_id LEFT JOIN sysobjects o WITH (NOLOCK) ON c.id=o.id LEFT JOIN sys.schemas u WITH (NOLOCK) ON o.uid = u.schema_id";


        internal static DataTable GetSPScripts(ServerInfo Server)
        {
            var data = SQLHelper.Query(SQLObjectScripts + " WHERE s.type = 'P' ORDER BY s.name", Server);
            var result = data.Clone();
            data.Rows.Cast<DataRow>().GroupBy(r => r.Field<string>("name")).ForEach(g => 
                {
                    var text = new StringBuilder();
                    var row = result.NewRow();
                    var first = g.First();
                    row.ItemArray = first.ItemArray;
                    row["text"] = g.Aggregate(new StringBuilder(), (a, b) => a.Append("\r\n" + b.Field<string>("text")), (a) => a.Remove(0,2).ToString()); 

                    result.Rows.Add(row);
                });
            return result;
        }

        internal static DataTable GetLockedProcesses(ServerInfo Server)
        {
            return SQLHelper.Query(SQLLockedProcesses, Server);
        }

        internal static DataTable GetSessions(ServerInfo Server)
        {
            return SQLHelper.Query(SQLSessions, Server);
        }

        internal static DataTable GetLockedObjects(short SessionId, ServerInfo Server)
        {
            var sql = SQLLockedObjects + " AND l.request_session_id = " + SessionId;
            return SQLHelper.Query(sql, Server);
        }

        internal static string GetSessionSQL(string SessionId, ServerInfo Server)
        {
            var data = SQLHelper.Query("dbcc INPUTBUFFER(" + SessionId + ")", Server);
            var sql = data != null && data.Rows.Count > 0 ? (data.Rows[0][2] as string) : string.Empty;
            sql = !string.IsNullOrEmpty(sql) ? sql.Replace("\0", string.Empty) : string.Empty;
            data = SQLHelper.Query(@"declare @s nvarchar(max)
declare @handle binary(20)
declare @start int
declare @end int
select @handle = sql_handle,@start = stmt_start, @end = stmt_end from sys.sysprocesses where spid=" + SessionId + @"
select @s = text FROM sys.dm_exec_sql_text( @handle )
select @s as FullStatement, SUBSTRING(@s, (@start/2)+1, ((CASE @end WHEN -1 THEN DATALENGTH(@s) ELSE @end END - @start)/2)+1) as CurrentStatement", Server);
            {
                if (data.Rows.Count > 0)
                {
                    sql = sql.Trim();
                    var full = data.Rows[0]["FullStatement"] as string;
                    if (!string.IsNullOrEmpty(full) && !string.IsNullOrEmpty(full.Trim()))
                    {
                        full = full.Trim();
                        var statement = data.Rows[0]["CurrentStatement"] as string;
                        if (!string.IsNullOrEmpty(statement) && !string.IsNullOrEmpty(statement.Trim()))
                        {
                            statement = statement.Trim();
                            if (statement != sql)
                            {
                                var finalSQL = "--actual command\r\n" + sql + "\r\n\r\n--current statement\r\n" + statement;
                                if (full != sql)
                                    finalSQL += "\r\n\r\n--full sql\r\n" + full;
                                sql = finalSQL;
                            }
                        }
                    }
                }
            }
            return sql;
        }

        internal static DataTable GetDatabaseIOInfo(ServerInfo Server)
        {
            var dataFiles = SQLHelper.Query(string.Format(@"SELECT sys.databases.database_id AS dbid, sys.master_files.file_id AS fileid, sys.master_files.physical_name AS filename
FROM sys.master_files INNER JOIN sys.databases 
ON sys.master_files.database_id = sys.databases.database_id 
WHERE sys.databases.name = '{0}'", Server.Database), Server);
            var data = new DataTable();
            data.Columns.Add("StartDate", typeof(DateTime));
            data.Columns.Add("IsStall", typeof(double));
            data.Columns.Add("IsReadStall", typeof(double));
            data.Columns.Add("IsWriteStall", typeof(double));
            data.Columns.Add("NumberReads", typeof(long));
            data.Columns.Add("BytesRead", typeof(long));
            data.Columns.Add("NumberWrites", typeof(long));
            data.Columns.Add("BytesWritten", typeof(long));
            data.Columns.Add("CurrentNumberReads", typeof(long));
            data.Columns.Add("CurrentNumberWrites", typeof(long));
            data.Columns.Add("IsLog", typeof(bool));
            data.Columns.Add("FileCount", typeof(long));
            data.Columns.Add("IoStallReadMS", typeof(long));
            data.Columns.Add("IoStallWriteMS", typeof(long));
            for (int i = 0; i < 2; i++)
            {
                var row = data.NewRow();
                row.ItemArray = new object[] { DateTime.Now, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                row["IsLog"] = Convert.ToBoolean(i);
                data.Rows.Add(row);
            }
            dataFiles.AsEnumerable().ForEach(d =>
            {
                var dbID = d["dbid"];
                var fileID = d["fileid"];
                var fileName = d["filename"].ToString();
                var index = Path.GetExtension(fileName).ToLower() == ".ldf" ? 1 : 0;
                //sys.dm_io_virtual_file_stats()
                var fileStats = SQLHelper.Query(string.Format("SELECT DATEADD(ss, -1 * Timestamp/1000 , getdate()) AS StartDate, IoStallReadMS, IoStallWriteMS, NumberReads, BytesRead, NumberWrites, BytesWritten FROM ::fn_virtualfilestats({0}, {1})", dbID, fileID), Server);
                fileStats.AsEnumerable().ForEach(f =>
                {
                    data.Rows[index]["StartDate"] = f["StartDate"];
                    data.Rows[index]["NumberReads"] = Convert.ToInt64(data.Rows[index]["NumberReads"]) + Convert.ToInt64(f["NumberReads"]);
                    if (Convert.ToInt64(data.Rows[index]["NumberReads"]) == 0)
                        data.Rows[index]["NumberReads"] = 1;
                    data.Rows[index]["BytesRead"] = Convert.ToInt64(data.Rows[index]["BytesRead"]) + Convert.ToInt64(f["BytesRead"]);
                    data.Rows[index]["NumberWrites"] = Convert.ToInt64(data.Rows[index]["NumberWrites"]) + Convert.ToInt64(f["NumberWrites"]);
                    if (Convert.ToDouble(data.Rows[index]["NumberWrites"]) == 0)
                        data.Rows[index]["NumberWrites"] = 1;
                    data.Rows[index]["BytesWritten"] = Convert.ToInt64(data.Rows[index]["BytesWritten"]) + Convert.ToInt64(f["BytesWritten"]);
                    data.Rows[index]["FileCount"] = Convert.ToInt64(data.Rows[index]["FileCount"]) + 1;
                    data.Rows[index]["IoStallReadMS"] = Convert.ToInt64(data.Rows[index]["IoStallReadMS"]) + Convert.ToInt64(f["IoStallReadMS"]);
                    data.Rows[index]["IoStallWriteMS"] = Convert.ToInt64(data.Rows[index]["IoStallWriteMS"]) + Convert.ToInt64(f["IoStallWriteMS"]);

                    data.Rows[index]["IsReadStall"] = Convert.ToDouble(data.Rows[index]["IoStallReadMS"]) / Convert.ToInt64(data.Rows[index]["NumberReads"]);
                    data.Rows[index]["IsWriteStall"] = Convert.ToDouble(data.Rows[index]["IoStallWriteMS"]) / Convert.ToInt64(data.Rows[index]["NumberWrites"]);
                    data.Rows[index]["IsStall"] = (Convert.ToDouble(data.Rows[index]["IoStallReadMS"]) + Convert.ToDouble(data.Rows[index]["IoStallWriteMS"])) / (Convert.ToInt64(data.Rows[index]["NumberReads"]) + Convert.ToInt64(data.Rows[index]["NumberWrites"]));
                });
            });
            return data;
        }

        public static string GetObjectName(object SchemaName, string ObjectName)
        {
            if (SchemaName != null && !string.IsNullOrEmpty(SchemaName.ToString()))
                return string.Format("{0}{1}{2}", SchemaName, Dot, ObjectName);
            else
                return ObjectName;
        }

        public static string ParseObjectName(string ObjectName, out string SchemaName)
        {
            int pos = ObjectName.IndexOf(Dot);
            if (pos != -1)
            {
                SchemaName = ObjectName.Substring(0, pos);
                return ObjectName.Substring(pos + 1);
            }
            else
            {
                SchemaName = "dbo";
                return ObjectName;
            }
        }

    }
}
