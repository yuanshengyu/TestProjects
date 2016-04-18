using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Reflection;

namespace Xnlab.SQLMon
{
    public enum ActivityTypes
    {
        Process = 0,
        Job = 1
    }

    public enum AlertTypes
    {
        SQL = 0,
        Server = 1,
        //CPU = 2,
        //Memory = 3,
        //Diskspace = 4
    }

    public enum AuthTypes
    {
        Windows = 1,
        SQLServer = 0
    }

    public enum AnalysisResultTypes
    {
        DiskFreeSpace = 1,
        DatabaseLogSpace = 2,
        TableIndexSpace = 3,
        TableIndexUsage = 4,
        Performance = 5,
        Fault = 6,
        None = 0
    }

    public enum DatabaseFileTypes : int
    {
        Data = 0,
        Log = 1
    }

    public enum TableIndexSpaceRules : int
    {
        DataIndexSpaceRatio = 0,
        DatabaseTableSpaceRatio = 1,
        IndexEfficency = 2
    }

    public class AnalysisResult
    {
        public AnalysisResultTypes ResultType { get; set; }
        public string ObjectName { get; set; }
        public decimal ReferenceValue { get; set; }
        public decimal CurrentValue { get; set; }
        public string Factor { get; set; }
        public long Key { get; set; }
        public string RuntimeValue { get; set; }
    }

    [Serializable]
    public class ServerInfoEx : ServerInfo
    {
        public bool IsServer { get; set; }
    }

    [Serializable]
    public class ServerInfo
    {
        public AuthTypes AuthType { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public bool IsEncrypted { get; set; }
        public string Server { get; set; }
        public string Database { get; set; }

        public ServerInfo()
        {
            IsEncrypted = false;
        }

        public override string ToString()
        {
            return Server + "," + Database + "," + User;
        }

        public ServerInfo Clone()
        {
            return new ServerInfo { Server = this.Server, AuthType = this.AuthType, Database = this.Database, User = this.User, Password = this.Password, IsEncrypted = this.IsEncrypted };
        }
    }

    [Serializable]
    public class MonitorItem
    {
        public string Server { get; set; }
        public AlertTypes AlertType { get; set; }
        public string Target { get; set; }
        public int CondictionType { get; set; }
        public int CondictionValue { get; set; }
        public AlertMethods AlertMethod { get; set; }
        public string Title { get; set; }
        public bool IsEnabled { get; set; }

        public MonitorItem()
        {
            IsEnabled = true;
        }

        public override string ToString()
        {
            return Server + ", " + AlertType + ", " + CondictionType + ", " + CondictionValue + ", " + Target;
        }
    }

    [Serializable]
    public class NotifiedMonitorItem
    {
        public string Server { get; set; }
        public string CurrentValue { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    internal class ServerState : ServerInfo
    {
        internal string Key { get; set; }
        internal bool IsReady { get; set; }
        internal bool State { get; set; }
    }

    public enum ActivityStatuses
    {
        Running = 0,
        Sleeping = 1,
        Suspended = 2,
        Background = 3,
        Runnable = 4,
        All = 5
    }

    public enum AlertMethods
    {
        MsgBox = 0,
        Mail = 1,
        Log = 2,
    }

    [Serializable]
    public class SerializableFont
    {
        public string Name { get; set; }
        public float Size { get; set; }
        public bool Bold { get; set; }
    }

    [Serializable]
    public class Settings
    {
        public const string DefaultTemplate = "#Type# #Action# \r\n raised on #Server# at #Now#";
        public List<ServerInfo> Servers = new List<ServerInfo>();
        public List<NotifiedMonitorItem> NotifiedAlerts = new List<NotifiedMonitorItem>();
        public List<MonitorItem> MonitorItems = new List<MonitorItem>();
        public List<ServerInfoEx> PerformanceItems = new List<ServerInfoEx>();
        public ActivityStatuses ActivityState { get; set; }
        public string LastQuery { get; set; }
        public string LastServer { get; set; }
        public List<string> SearchHistories = new List<string>();
        public string LastSearchContent { get; set; }
        public bool LastSearchIsCaseSenstive { get; set; }
        public bool LastSearchIsObject { get; set; }
        public ActivityTypes ActivityType { get; set; }
        public int MonitorRefreshInterval { get; set; }
        public int PerformanceInterval { get; set; }
        public SerializableFont EditorFont { get; set; }
        public bool LogHistory { get; set; }
        public bool AutoWordWrap { get; set; }
        public string AlertMailServer { get; set; }
        public string AlertPort { get; set; }
        public string AlertHost { get; set; }
        public string AlertMailUser { get; set; }
        public string AlertMailPassword { get; set; }
        public string AlertMailReceiver { get; set; }
        public string AlertTemplate { get; set; }
        public string VersionControlTableName { get; set; }
        public string VersionControlTriggerName { get; set; }
        public int ConnectionTimeout { get; set; }
        public int ObjectsSplitterDistance { get; set; }
        public int MainSplitterDistance { get; set; }
        public int DatabaseDiskFreeSpaceRatio { get; set; }
        public int DatabaseDataLogSpaceRatio { get; set; }
        public int TableDataIndexSpaceRatio { get; set; }

        public Settings()
        {
            ActivityState = ActivityStatuses.All;
            EditorFont = new SerializableFont { Name = "Tahoma", Size = 10, Bold = false };
            ActivityType = ActivityTypes.Process;
            VersionControlTableName = "SQLMonSystemObjectVersionControls";
            VersionControlTriggerName = "trg_SQLMonSystemObjectVersionControls";
            LastSearchIsObject = true;
            MonitorRefreshInterval = 10;
            PerformanceInterval = 5;
            LogHistory = true;
            AutoWordWrap = true;
            ConnectionTimeout = 30;
            DatabaseDiskFreeSpaceRatio = 30;
            DatabaseDataLogSpaceRatio = 40;
            TableDataIndexSpaceRatio = 100;
        }

        private static Settings settings = null;

        public static string SettingsFile
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\SQLMon.cfg"; }
        }

        internal static string Title
        {
            get { return ((Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]) as AssemblyTitleAttribute).Title; }
        }

        internal void AddPerformanceItem(ServerInfo Server, bool IsServer)
        {
            if (FindPerformanceServer(Server, IsServer) == null)
                PerformanceItems.Add(new ServerInfoEx { Database = Server.Database, Server = Server.Server, IsServer = IsServer });
        }

        internal void RemovePerformanceItem(ServerInfo Server, bool IsServer)
        {
            var item = FindPerformanceServer(Server, IsServer);
            if (item != null)
                PerformanceItems.Remove(item);
        }

        private ServerInfoEx FindPerformanceServer(ServerInfo Server, bool IsServer)
        {
            return PerformanceItems.FirstOrDefault(s => s.Server == Server.Server
                && s.Database == Server.Database
                && s.IsServer == IsServer);
        }

        public static Settings Instance
        {
            get
            {
                if (settings == null)
                {
                    settings = new Settings();
                    if (File.Exists(SettingsFile))
                    {
                        var serializer = new XmlSerializer(typeof(Settings));
                        using (var reader = File.OpenText(SettingsFile))
                        {
                            settings = (Settings)serializer.Deserialize(reader);

                            settings.Servers.ForEach(s =>
                                {
                                    if (s.IsEncrypted)
                                    {
                                        try
                                        {
                                            s.Password = AES.Decrypt(s.Password);
                                            s.IsEncrypted = false;
                                        }
                                        catch (Exception)
                                        {
                                        }
                                    }
                                });
                        }
                    }
                }
                return settings;
            }
        }

        public void Save()
        {
            if (File.Exists(SettingsFile))
                File.Delete(SettingsFile);
            var serializer = new XmlSerializer(typeof(Settings));
            var settings = Utils.CloneObject<Settings>(this);
            using (var writer = File.OpenWrite(SettingsFile))
            {
                settings.Servers.ForEach(s =>
                {
                    if (!s.IsEncrypted)
                    {
                        s.Password = AES.Encrypt(s.Password);
                        s.IsEncrypted = true;
                    }
                });
                serializer.Serialize(writer, settings);
            }
        }

        public ServerInfo FindServer(string Server)
        {
            return Servers.FirstOrDefault((s) => s.Server.ToLower() == Server.ToLower());
        }

        public ServerInfo FindServer(string Server, string User)
        {
            return Servers.FirstOrDefault((s) => s.Server.ToLower() == Server.ToLower()
                    && s.User == User);
        }
    }
}
