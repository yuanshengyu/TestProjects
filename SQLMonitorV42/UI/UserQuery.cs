using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace Xnlab.SQLMon
{
    public partial class UserQuery : UserControl, ICancelable
    {
        private ServerInfo server = null;
        private string fileName = string.Empty;
        private bool isRunning = false;
        private Thread thread = null;

        public UserQuery()
        {
            InitializeComponent();
        }

        public UserQuery(string Query, ServerInfo Server)
            : this()
        {
            server = Server.Clone();
            rtbSQL.Font = Monitor.Instance.SetFont();
            Utils.SetTextBoxStyle(rtbSQL);
            rtbSQL.Text = Query;
        }

        ~UserQuery()
        {
            Cancel();
        }

        public void Cancel()
        {
            try
            {
                if (isRunning && thread != null)
                    thread.Abort();
                isRunning = false;
            }
            catch (Exception)
            {
            }
        }

        public bool IsRunning
        {
            get { return isRunning; }
        }

        private void SetCommand(bool Cancel)
        {
            isRunning = Cancel;
            Monitor.Instance.SetExecute(Cancel);
        }

        private void StartQuery(object State)
        {
            try
            {
                SetCommand(true);
//#if (DEBUG)
//                Thread.Sleep(10000);
//#endif
                string message;
                var time = new System.Diagnostics.Stopwatch();
                time.Start();
                var results = SQLHelper.QuerySet((string)State, server, out message);
                if (results != null)
                {
                    this.Invoke(() =>
                    {
                        results.Tables.Cast<DataTable>().ForEach(t =>
                        {
                            var dataGrid = new DataGridView();
                            dataGrid.DataError += new DataGridViewDataErrorEventHandler(OnQueryDataGridDataError);
                            dataGrid.Location = new Point(0, tpData.Controls.Cast<DataGridView>().Sum((c) => c.Height + 6));
                            dataGrid.Width = tpData.Width - 20;
                            dataGrid.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
                            dataGrid.ReadOnly = true;
                            dataGrid.AllowUserToAddRows = false;
                            dataGrid.AllowUserToDeleteRows = false;
                            dataGrid.DataSource = t;
                            tpData.Controls.Add(dataGrid);
                            for (int i = 0; i < dataGrid.Rows.Count; i++)
                            {
                                dataGrid.Rows[i].HeaderCell.Value = (i + 1).ToString();
                                dataGrid.Rows[i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
                            }
                        });
                        if (tpData.Controls.Count > 0)
                        {
                            int defaultHeight = 160;
                            int height = tpData.Height / tpData.Controls.Count;
                            if (height < defaultHeight)
                                height = defaultHeight;
                            tpData.Controls.Cast<Control>().ForEach(c => c.Height = height);
                        }
                        time.Stop();
                        rtbInfo.Text = message + "\r\n\r\nExecuted in " + time.Elapsed;
                    });
                }
            }
            catch (Exception ex)
            {
                this.Invoke(() =>
                    {
                        tcQueryResult.SelectedTab = tpInfo;
                        rtbInfo.Text = ex is ThreadAbortException ? "Query cancelled." :  ex.Message;
                    });
            }
            finally
            {
                SetCommand(false);
            }
        }

        public void Execute()
        {
            if (!isRunning)
            {
                tpData.Controls.Clear();
                var sql = rtbSQL.ActiveTextAreaControl.TextArea.SelectionManager.SelectedText;
                if (string.IsNullOrEmpty(sql))
                    sql = rtbSQL.Text;
                if (!string.IsNullOrEmpty(sql))
                {
                    Settings.Instance.LastQuery = sql;
                    using (new DisposableState(this, Monitor.Instance.Commands))
                    {
                        thread = new Thread(new ParameterizedThreadStart(StartQuery));
                        thread.Start(sql);
                    }
                }
                else
                    Monitor.Instance.ShowMessage("Please input sql to execute.");
            }
            else
            {
                if (thread != null)
                    thread.Abort();
                isRunning = false;
                SetCommand(false);
            }
        }

        private void OnQueryDataGridDataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
        }

        private void OnChangeConnectionClick(object sender, EventArgs e)
        {
            using (ConnectionDialog dlg = new ConnectionDialog(server))
            {
                if (dlg.ShowDialog(this.ParentForm) == DialogResult.OK)
                {
                    server.Server = dlg.Server;
                    server.User = dlg.UserName;
                    server.Password = dlg.Password;
                    server.AuthType = dlg.AuthType;
                    var page = this.Parent as TabPage;
                    var index = page.Text.IndexOf(" ");
                    page.Text = server.Server + page.Text.Substring(index);
                }
            }
        }

        private void OnSaveScriptClick(object sender, EventArgs e)
        {
            SaveScript();
        }

        private void SaveScript()
        {
            if (string.IsNullOrEmpty(fileName))
            {
                var page = this.Parent as TabPage;
                fileName = page.Text;
                Path.GetInvalidFileNameChars().ForEach(c => fileName = fileName.Replace(c.ToString(), string.Empty));
                Path.GetInvalidPathChars().ForEach(c => fileName = fileName.Replace(c.ToString(), string.Empty));
            }
            Monitor.Instance.SaveScript(fileName, rtbSQL.Text);
        }

        private void OnOpenScriptClick(object sender, EventArgs e)
        {
            OpenScript();
        }

        private void OpenScript()
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                if (dlg.ShowDialog(this.ParentForm) == DialogResult.OK)
                {
                    fileName = dlg.FileName;
                    var text = File.ReadAllText(fileName);
                    if (!string.IsNullOrEmpty(rtbSQL.ActiveTextAreaControl.TextArea.SelectionManager.SelectedText))
                        Utils.SelectText(rtbSQL, text);
                    else
                        rtbSQL.Text = text;
                }
            }
        }

        private void OnSQLKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.S:
                    if (e.Control)
                        SaveScript();
                    break;
                case Keys.O:
                    if (e.Control)
                        OpenScript();
                    break;
                default:
                    break;
            }
        }

        private void OnCompareScriptClick(object sender, EventArgs e)
        {
            using (var dlg = new DiffResults(rtbSQL.Text))
            {
                dlg.ShowDialog(this);
            }
        }

        private void OnContentDragDrop(object sender, DragEventArgs e)
        {
            var result = Utils.GetDragDropContent(e);
            if (!string.IsNullOrEmpty(result))
                rtbSQL.Text = result;
        }

        private void OnContentDragEnter(object sender, DragEventArgs e)
        {
            Utils.HandleContentDragEnter(e);
        }
    }
}
