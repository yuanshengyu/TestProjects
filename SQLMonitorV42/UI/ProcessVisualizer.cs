using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using TreeGenerator;

namespace Xnlab.SQLMon
{
    public partial class ProcessVisualizer : BaseDialog
    {
        private const string KeyScheduler = "S";
        private const string KeyWorker = "W";
        private const string KeyTask = "T";
        private const string KeyRequest = "R";
        private TreeBuilder tree = null;
        private DataTable processes = null;
        private int currentProcess = 0;
        private readonly ServerInfo serverInfo = null;

        public ProcessVisualizer()
        {
            InitializeComponent();
            this.Description = "Single click a process to view info, double click a process to kill";
            Utils.SetTextBoxStyle(rtbInfo);
        }

        public ProcessVisualizer(ServerInfo Server)
            : this()
        {
            if (Server != null)
                serverInfo = Server.Clone();
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            LoadProcesses();
        }

        internal ServerInfo Server
        {
            get { return serverInfo; }
        }

        internal void LoadProcesses()
        {
            rtbInfo.Text = string.Empty;
            rtbInfo.Refresh();
            cmdKill.Enabled = false;
            var model = GetProcesses();
            tree = new TreeBuilder(model);
            tree.BGColor = this.BackColor;
            if (model.Count > 0)
                picProcesses.Image = Image.FromStream(tree.GenerateTree(-1, -1, model.First.ID, System.Drawing.Imaging.ImageFormat.Png));
            else
                picProcesses.Image = null;
        }

        private void OnProccessMouseClick(object sender, MouseEventArgs e)
        {
            var node = GetCurrentNode(e.X, e.Y);
            string description = string.Empty;
            if (node != null)
            {
                description = node.Attributes["nodeDescription"].Value;
                var id = node.Attributes["nodeID"].Value;
                if (id.StartsWith(KeyRequest))
                    currentProcess = Convert.ToInt32(id.Substring(1));
                else
                    currentProcess = 0;
            }
            else
                currentProcess = 0;
            rtbInfo.Text = string.Format(description);
            rtbInfo.Refresh();
            cmdKill.Enabled = currentProcess != 0;
        }

        private XmlNode GetCurrentNode(int X, int Y)
        {
            XmlNode node = null;
            Rectangle currentRect;
            //find the node
            foreach (XmlNode oNode in tree.xmlTree.SelectNodes("//Node"))
            {
                //iterate through all nodes until found.
                currentRect = tree.getRectangleFromNode(oNode);
                if (X >= currentRect.Left &&
                    X <= currentRect.Right &&
                    Y >= currentRect.Top &&
                    Y <= currentRect.Bottom)
                {
                    node = oNode;
                    break;
                }
            }
            return node;
        }

        private void OnProcessesMouseMove(object sender, MouseEventArgs e)
        {
            var cursor = Cursors.Default;
            var node = GetCurrentNode(e.X, e.Y);
            if (node != null)
                cursor = Cursors.Hand;
            if (picProcesses.Cursor != cursor)
            {
                picProcesses.Cursor = cursor;
                ttHint.SetToolTip(picProcesses.InnerPicture, node != null ? node.Attributes["nodeDescription"].Value : string.Empty);
            }
        }

        private Tree GetProcesses()
        {
            var model = new Tree();
            if (serverInfo != null)
            {
                try
                {
                    var server = serverInfo.Server;
                    var rootID = server;
                    model.AddNode(rootID, string.Empty, server, server, server);

                    var taskList = new Dictionary<byte[], int>();
                    processes = QueryEngine.GetSessions(serverInfo);
                    var rows = processes.AsEnumerable().DistinctBy(r => r.Field<short>("session_id"));
                    var schedulers = rows.GroupBy(r => r.Field<byte[]>("scheduler_address"));
                    var schedulerList = new List<string>();
                    var workerID = 0;
                    var taskID = 0;
                    Color blockColor = Color.Salmon;
                    schedulers.ForEach(s =>
                        {
                            //http://msdn.microsoft.com/en-us/library/ms177526.aspx
                            var scheduler = s.First();
                            var schedulerID = scheduler.Field<int>("scheduler_id").ToString();
                            string description;
                            var schedulerRoot = KeyScheduler + schedulerID;
                            if (!schedulerList.Contains(schedulerID))
                            {
                                schedulerList.Add(schedulerID);
                                var isIdle = scheduler.Field<bool>("is_idle");
                                description = string.Format("is idle: {0}\r\ncurrent workers: {1}\r\nactive workers: {2}", isIdle.ToString(), scheduler.Field<int>("current_workers_count").ToString(), scheduler.Field<int>("active_workers_count"));
                                model.AddNode(schedulerRoot, rootID, schedulerRoot, "Scheduler " + schedulerID, description, isIdle ? Color.LightGray : Color.White);
                            }
                            var workers = s.GroupBy(w => w.Field<byte[]>("worker_address"));
                            workers.ForEach(w =>
                                {
                                    //http://msdn.microsoft.com/en-us/library/ms178626.aspx
                                    var workerRoot = KeyWorker + workerID;
                                    var worker = w.First();
                                    var workerState = worker.Field<string>("worker_state");
                                    var workerStateName = string.Empty;
                                    Color workerColor = Color.White;
                                    switch (workerState.ToUpper())
                                    {
                                        case "INIT":
                                            workerStateName = "initializing";
                                            workerColor = Color.LightGreen;
                                            break;
                                        case "RUNNING":
                                            workerStateName = "running";
                                            workerColor = Color.LightBlue;
                                            break;
                                        case "RUNNABLE":
                                            workerStateName = "runnable";
                                            workerColor = Color.Green;
                                            break;
                                        case "SUSPENDED":
                                            workerStateName = "suspended";
                                            workerColor = Color.LightGray;
                                            break;
                                        default:
                                            workerStateName = "unkown";
                                            break;
                                    }
                                    var workerIsSick = worker.Field<bool>("is_sick");
                                    var workerIsFatalException = worker.Field<bool>("is_fatal_exception");
                                    var workerExceptionCount = worker.Field<int>("exception_num");
                                    var workerTasksProcessedCount = worker.Field<int>("tasks_processed_count");
                                    var workerLastResult = worker.Field<int>("return_code");
                                    var workerLastResultName = string.Empty;
                                    switch (workerLastResult)
                                    {
                                        case 0:
                                            workerLastResultName = "success";
                                            break;
                                        case 3:
                                            workerLastResultName = "dead lock";
                                            break;
                                        case 4:
                                            workerLastResultName = "premature wakeup";
                                            break;
                                        case 258:
                                            workerLastResultName = "time out";
                                            break;
                                        default:
                                            workerLastResultName = string.Format("(unkown:{0})", workerLastResult);
                                            break;
                                    }
                                    description = string.Format("processed tasks: {0}\r\nstate: {1}\r\nis sick: {2}\r\nis fatal exception:{3}\r\nexception number:{4}\r\nlast result:{5}", workerTasksProcessedCount, workerStateName, workerIsSick.ToString(), workerIsFatalException.ToString(), workerExceptionCount, workerLastResultName);

                                    model.AddNode(workerRoot, schedulerRoot, workerRoot, "Worker " + workerID, description, workerColor);

                                    var tasks = w.GroupBy(t => t.Field<byte[]>("task_address"));
                                    tasks.ForEach(t =>
                                        {
                                            //http://msdn.microsoft.com/en-us/library/ms174963.aspx
                                            var taskRoot = KeyTask + taskID;
                                            var task = t.First();
                                            var taskAddress = task.Field<byte[]>("task_address");
                                            if (!taskList.ContainsKey(taskAddress))
                                                taskList.Add(taskAddress, taskID);
                                            var taskState = task.Field<string>("task_state");
                                            var taskStateName = string.Empty;
                                            Color taskColor = Color.White;
                                            switch (taskState.ToUpper())
                                            {
                                                case "PENDING":
                                                    taskStateName = "pending";
                                                    taskColor = Color.LightGreen;
                                                    break;
                                                case "RUNNING":
                                                    taskStateName = "running";
                                                    taskColor = Color.LightBlue;
                                                    break;
                                                case "RUNNABLE":
                                                    taskStateName = "runnable";
                                                    taskColor = Color.Green;
                                                    break;
                                                case "SUSPENDED":
                                                    taskStateName = "suspended";
                                                    taskColor = Color.LightGray;
                                                    break;
                                                case "DONE":
                                                    taskStateName = "done";
                                                    taskColor = Color.White;
                                                    break;
                                                case "SPINLOOP":
                                                    taskStateName = "spin loop";
                                                    taskColor = Color.LightSalmon;
                                                    break;
                                                default:
                                                    taskStateName = "unknown";
                                                    break;
                                            }
                                            description = string.Format("state:" + taskStateName);
                                            model.AddNode(taskRoot, workerRoot, taskRoot, "Task " + taskID, description, taskColor);

                                            taskID++;
                                        });

                                    workerID++;
                                });
                        });


                    var addedProcesses = new List<short>();
                    var lockedProcesses = QueryEngine.GetLockedProcesses(serverInfo);
                    lockedProcesses.AsEnumerable().ForEach(p =>
                        {
                            var spid = p.Field<short>("SPID");
                            var task = rows.FirstOrDefault(r => r.Field<short>("session_id") == spid);
                            if (task != null)
                            {
                                var taskAddress = task.Field<byte[]>("task_address");
                                int parent;
                                if (taskList.TryGetValue(taskAddress, out parent))
                                {
                                    var taskRoot = KeyTask + parent;
                                    var requstID = KeyRequest + spid;
                                    var description = GetProcessDescription(task);
                                    var blockingID = p.Field<short?>("BlockingSPID");
                                    Color backColor;
                                    if (blockingID != null && blockingID != 0)
                                    {
                                        taskRoot = KeyRequest + blockingID;
                                        backColor = blockColor;
                                    }
                                    else
                                    {
                                        backColor = Color.Red;
                                        description = "!blocking root, kill it!\r\n\r\n" + description;

                                    }
                                    var currentServer = serverInfo.Clone();
                                    currentServer.Database = task.Field<string>("database_name");
                                    var lockedObjects = QueryEngine.GetLockedObjects(spid, currentServer);
                                    var lockedObjectList = new StringBuilder();
                                    if (lockedObjects.Rows.Count > 0)
                                    {
                                        var sessionLockedObjects = lockedObjects.AsEnumerable();
                                        if (sessionLockedObjects.Any())
                                        {
                                            lockedObjectList.AppendLine("locked objects:");
                                            sessionLockedObjects.ForEach(r =>
                                                {
                                                    lockedObjectList.AppendFormat("  {0}.{1}.{2}\r\n", r.Field<string>("DatabaseName"), r.Field<string>("SchemaName"), r.Field<string>("ObjectName"));
                                                });
                                        }
                                    }
                                    description = lockedObjectList.ToString() + description;
                                    addedProcesses.Add(spid);
                                    model.AddNode(requstID, taskRoot, requstID, "Process " + spid, description, backColor);
                                }
                            }
                        });


                    rows.ForEach(p =>
                    {
                        var sessionID = p.Field<short>("session_id");
                        if (!addedProcesses.Contains(sessionID))
                        {
                            var session = KeyRequest + sessionID;
                            var taskAddress = p.Field<byte[]>("task_address");
                            var task = taskList[taskAddress];
                            var taskRoot = KeyTask + task;
                            var description = GetProcessDescription(p);
                            Color backColor;
                            var sessionStatus = p.Field<string>("session_status");
                            switch (sessionStatus.ToUpper())
                            {
                                case "RUNNING":
                                    backColor = Color.LightBlue;
                                    break;
                                case "SLEEPING":
                                    backColor = Color.LightGray;
                                    break;
                                case "DORMANT":
                                    backColor = Color.Gray;
                                    break;
                                case "PRECONNECT":
                                    backColor = Color.Gray;
                                    break;
                                default:
                                    backColor = Color.White;
                                    break;
                            }
                            model.AddNode(session, taskRoot, session, "Process " + sessionID, description, backColor);
                        }
                    });

                    var title = "Process Visualizer ({0})";
                    if (lockedProcesses.Rows.Count > 0)
                        title += ", found {1} dead lock processes!";
                    this.Text = string.Format(title, serverInfo.Server, lockedProcesses.Rows.Count);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            return model;
        }

        private string GetProcessDescription(DataRow Row)
        {
            var sessionStatus = Row.Field<string>("session_status");
            var openTransactionCount = Row.Field<int>("open_transaction_count");
            var command = Row.Field<string>("command");
            var databaseName = Row.Field<string>("database_name");
            var currentStatement = Row.Field<string>("current_statement");
            var text = currentStatement ?? Row.Field<string>("text");
            return string.Format("status:{0}\r\nopen transaction count:{1}\r\ndatabase:{2}\r\ncommand:{3}\r\nexecuting statement:\r\n\r\n{4}", sessionStatus, openTransactionCount, databaseName, command, text);
        }

        private void OnProcessMouseDoubleClick(object sender, MouseEventArgs e)
        {
            Kill();
        }

        private void OnKillClick(object sender, EventArgs e)
        {
            Kill();
        }

        private void Kill()
        {
            if (currentProcess != 0)
            {
                if (MessageBox.Show(this, "Are you sure to kill the selected process?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    SQLHelper.ExecuteNonQuery(string.Format("kill {0}", currentProcess), serverInfo);
                    LoadProcesses();
                }
            }
        }

        private void OnViewClick(object sender, EventArgs e)
        {
            Monitor.Instance.ShowActivities();
            Monitor.Instance.BringToFront();
        }

        private void OnRefreshClick(object sender, EventArgs e)
        {
            LoadProcesses();
        }
    }
}
