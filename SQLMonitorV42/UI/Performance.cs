using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Xnlab.SQLMon
{
    public partial class Performance : UserControl
    {
        private ObjectModes objectMode;
        private ServerInfo server;
        private MonitorEngine engine;
        private bool isDocked = true;
        private int lastPointIndex = 0;
        private List<HistoryRecord> currentRecords = null;
        private bool isLoading = false;

        public Performance()
        {
            InitializeComponent();

            Enum.GetValues(typeof(DateTypes)).Cast<DateTypes>().ForEach((s) => cboPerformanceViewTypes.Items.Add(s));
            cboPerformanceViewTypes.SelectedItem = DateTypes.Day;

            dtpPerformanceStartDate.Value = DateTime.Now.Date;
        }

        public void Init(ObjectModes ObjectMode, ServerInfo Server)
        {
            isLoading = true;
            objectMode = ObjectMode;
            server = Server;
            chkAutoPerformance.Checked = Settings.Instance.PerformanceItems.Exists(p => p.Server == server.Server
                && p.Database == server.Database && p.IsServer == IsServer);
            isLoading = false;
            StartEngine();
        }

        public void RemovePerformanceItem()
        {
            MonitorEngine.Instance.RemoveUserPerformanceItem(server, IsServer);
        }

        private void StartEngine()
        {
            if (engine == null)
            {
                engine = new MonitorEngine();
                engine.Message += new EventHandler<MessageEventArgs>(OnMonitorEngineMessage);
                engine.RequestServer += new EventHandler<ServerInfoEventArgs>(OnMonitorEngineRequestServer);
                engine.UpdateServerInfo += new EventHandler<PerformanceRecordEventArgs>(OnMonitorUpdateServerInfo);
            }
        }

        internal void ShowPopDock()
        {
            cmdPopDock.Visible = true;
        }

        private bool IsServer
        {
            get { return objectMode == ObjectModes.Server; }
        }

        internal string Title
        {
            get
            {
                var name = History.GetKey(server, IsServer);
                return "Performance (" + name + ")";
            }
        }

        internal ObjectModes ObjectMode
        {
            get { return objectMode; }
        }

        internal ServerInfo Server
        {
            get { return server; }
        }

        private void OnPerformanceCursorPositionChanged(object sender, System.Windows.Forms.DataVisualization.Charting.CursorEventArgs e)
        {
            if (chPerformance.Series.Count > 0 && currentRecords.Count > 0)
            {
                var index = currentRecords.FindIndex(p => p.Value16.ToOADate() >= e.NewPosition);
                if (lastPointIndex != index && index >= 0)
                {
                    lastPointIndex = index;
                    SetPerformanceInfo();
                }
                System.Diagnostics.Debug.WriteLine(e.NewPosition);
            }
        }

        private void OnMonitorEngineRequestServer(object sender, ServerInfoEventArgs e)
        {
            e.IsServer = objectMode == ObjectModes.Server;
            e.Server = server;
            e.Cancel = server == null;
        }

        internal void SetInterval(string Interval)
        {
            StartEngine();
            engine.SetPerformanceInterval(Interval);
        }

        private void OnMonitorEngineMessage(object sender, MessageEventArgs e)
        {
            this.Invoke(() =>
            {
                if (e.Cancel)
                {
                    engine.DisablePerformance();
                }
                Monitor.Instance.ShowMessage(e.Message);
            });
        }

        internal void ResetPerformance()
        {
            ResetPerformance(true);
        }

        private void ResetPerformance(bool IsNew)
        {
            chPerformance.Series.Clear();
            ResetPerformanceIO();
            txtPerformanceIO.Text = string.Empty;
            txtPerformanceCPU.Text = string.Empty;
            txtPerformanceRead.Text = string.Empty;
            txtPerformanceWrite.Text = string.Empty;
            txtPerformancePackets.Text = string.Empty;
            txtPerformanceConnections.Text = string.Empty;
            if (objectMode == ObjectModes.Databases)
            {
                llPerformanceIO.Text = "Stall";
                lblPerformanceCPU.Text = "Since";
                ttInfo.SetToolTip(txtPerformanceCPU, "Calculation since last startup");
                lblPerformanceRead.Text = "DB read";
                ttInfo.SetToolTip(txtPerformanceRead, "Number of reads / Total reads");
                lblPerformanceWrite.Text = "DB write";
                ttInfo.SetToolTip(txtPerformanceWrite, "Number of writes / Total writes");
                lblPerformancePacket.Text = "Log read";
                ttInfo.SetToolTip(txtPerformancePackets, "Number of reads / Total reads");
                lblPerformanceConnection.Text = "Log write";
                ttInfo.SetToolTip(txtPerformanceConnections, "Number of writes / Total writes");
            }
            else if (objectMode == ObjectModes.Server)
            {
                llPerformanceIO.Text = "IO";
                ttInfo.SetToolTip(txtPerformanceIO, "Total IO time / Current IO %");
                lblPerformanceCPU.Text = "CPU";
                ttInfo.SetToolTip(txtPerformanceCPU, "Total CPU time / Current CPU %");
                lblPerformanceRead.Text = "Read";
                ttInfo.SetToolTip(txtPerformanceRead, "Total number of reads since last startup/ Recent number of reads");
                lblPerformanceWrite.Text = "Write";
                ttInfo.SetToolTip(txtPerformanceWrite, "Total number of writes since last startup/ Recent number of writes");
                lblPerformancePacket.Text = "Packet";
                ttInfo.SetToolTip(txtPerformancePackets, "Total number of packets received / Total number of packets sent");
                lblPerformanceConnection.Text = "Connection";
                ttInfo.SetToolTip(txtPerformanceConnections, "Total connection count / Recent connection count");
            }
            if (IsNew)
                chkShowPerformanceHistory.Checked = false;
            lastPointIndex = 0;
            currentRecords = new List<HistoryRecord>();
        }

        private void ResetPerformanceIO()
        {
            txtPerformanceIO.BackColor = SystemColors.Control;
            ttInfo.SetToolTip(txtPerformanceIO, "DB stall / Log stall");
            llPerformanceIO.Enabled = false;
        }

        private Series AddPerformanceSerie(string Name, Color Color)
        {
            var serie = new Series(Name);
            serie.ChartArea = "Default";
            serie.BorderWidth = 1;
            serie.Color = Color;
            serie.ChartType = SeriesChartType.Line;
            serie.ShadowOffset = 1;
            serie.XValueType = ChartValueType.Time;
            //serie.LabelFormat = "HH:MM:SS";
            chPerformance.Series.Add(serie);
            return serie;
        }

        private void AddPerformanceValue(DateTime TimeStamp, Series Serie, long Value)
        {
            var current = TimeStamp.ToOADate();

            if (current < chPerformance.ChartAreas[0].AxisX.Minimum)
                Serie.Points.Clear();

            Serie.Points.AddXY(current, Value);
            //Serie.Points[Serie.Points.Count - 1].LabelFormat = "HH:MM:SS";

            if (!chkShowPerformanceHistory.Checked)
            {
                var removeBefore = TimeStamp.AddSeconds(-60).ToOADate();
                while (Serie.Points[0].XValue < removeBefore)
                {
                    Serie.Points.RemoveAt(0);
                }
                chPerformance.ChartAreas[0].AxisX.Minimum = Serie.Points[0].XValue;
                chPerformance.ChartAreas[0].AxisX.Maximum = DateTime.FromOADate(Serie.Points[0].XValue).AddMinutes(1).ToOADate();
            }

            chPerformance.Invalidate();
        }

        private void AddPerformanceHistoryRecord(PerformanceRecord Record, DateTime Date, DateTime Min, DateTime Max)
        {
            if (objectMode == ObjectModes.Server)
            {
                var cpu_busy_current = Record.Value1;
                var io_busy_current = Record.Value2;
                var current_read = Record.Value3;
                var current_write = Record.Value4;
                var packets_received_current = Record.Value5;
                var packets_sent_current = Record.Value6;
                var connections_current = Record.Value7;
                Series cpu;
                Series io;
                Series read;
                Series write;
                Series packets_sent;
                Series packets_received;
                Series connections;
                if (chPerformance.Series.Count == 0)
                {
                    chPerformance.ChartAreas[0].AxisX.Minimum = Min.ToOADate();
                    chPerformance.ChartAreas[0].AxisX.Maximum = Max.ToOADate();

                    cpu = AddPerformanceSerie("CPU", Color.Green);
                    io = AddPerformanceSerie("IO", Color.Blue);
                    read = AddPerformanceSerie("Read", Color.Red);
                    write = AddPerformanceSerie("Write", Color.Salmon);
                    packets_received = AddPerformanceSerie("Packet Received", Color.Tan);
                    packets_sent = AddPerformanceSerie("Packet Sent", Color.Pink);
                    connections = AddPerformanceSerie("Connections", Color.Yellow);
                }
                else
                {
                    cpu = chPerformance.Series[0];
                    io = chPerformance.Series[1];
                    read = chPerformance.Series[2];
                    write = chPerformance.Series[3];
                    packets_received = chPerformance.Series[4];
                    packets_sent = chPerformance.Series[5];
                    connections = chPerformance.Series[6];
                }
                AddPerformanceValue(Date, cpu, cpu_busy_current);
                AddPerformanceValue(Date, io, io_busy_current);
                AddPerformanceValue(Date, read, current_read);
                AddPerformanceValue(Date, write, current_write);
                AddPerformanceValue(Date, packets_received, packets_received_current);
                AddPerformanceValue(Date, packets_sent, packets_sent_current);
                AddPerformanceValue(Date, connections, connections_current);
            }
            else
            {
                var dbCurrentNumberReads = Record.Value5;
                var dbCurrentNumberWrites = Record.Value6;

                long logCurrentNumberReads = Record.Value11;
                long logCurrentNumberWrites = Record.Value12;

                Series dbReads;
                Series dbWrites;
                Series logReads;
                Series logWrites;
                if (chPerformance.Series.Count == 0)
                {
                    chPerformance.ChartAreas[0].AxisX.Minimum = Min.ToOADate();
                    chPerformance.ChartAreas[0].AxisX.Maximum = Max.ToOADate();

                    dbReads = AddPerformanceSerie("DB Read", Color.Red);
                    dbWrites = AddPerformanceSerie("DB Write", Color.Salmon);
                    logReads = AddPerformanceSerie("Log Read", Color.Green);
                    logWrites = AddPerformanceSerie("Log Write", Color.Blue);
                }
                else
                {
                    dbReads = chPerformance.Series[0];
                    dbWrites = chPerformance.Series[1];
                    logReads = chPerformance.Series[2];
                    logWrites = chPerformance.Series[3];
                }
                AddPerformanceValue(Date, dbReads, dbCurrentNumberReads);
                AddPerformanceValue(Date, dbWrites, dbCurrentNumberWrites);
                AddPerformanceValue(Date, logReads, logCurrentNumberReads);
                AddPerformanceValue(Date, logWrites, logCurrentNumberWrites);
            }
        }

        private void OnMonitorUpdateServerInfo(object sender, PerformanceRecordEventArgs e)
        {
            if (this.IsHandleCreated && !this.IsDisposed)
            {
                this.BeginInvoke((MethodInvoker)delegate()
                {
                    if (!chkShowPerformanceHistory.Checked)
                    {
                        try
                        {
                            PerformanceRecord record = e.Data;
                            var now = DateTime.Now;
                            var historyRecord = new HistoryRecord(record) { Date = now.ToString(), Value16 = now }; 
                            currentRecords.Add(historyRecord);
                            AddPerformanceHistoryRecord(record, now, now, now.AddMinutes(1));
                            SetPerformanceInfo(record);
                        }
                        catch (Exception ex)
                        {
                            engine.DisablePerformance();
                            Monitor.Instance.ShowMessage(ex.Message);
                        }
                    }
                });
            }
        }

        private void SetPerformanceInfo(PerformanceRecord Record)
        {
            isLoading = true;
            if (objectMode == ObjectModes.Server)
            {
                var cpu_busy_current = Record.Value1;
                var io_busy_current = Record.Value2;
                var current_read = Record.Value3;
                var current_write = Record.Value4;
                var connections_current = Record.Value7;
                var io_busy_total = Record.Value8;
                txtPerformanceIO.Text = string.Format("{0} / {1}", io_busy_total, io_busy_current);

                var cpu_busy_total = Record.Value9;
                txtPerformanceCPU.Text = string.Format("{0} / {1}", cpu_busy_total, cpu_busy_current);

                var total_read = Record.Value10;
                txtPerformanceRead.Text = string.Format("{0} / {1}", total_read, current_read);

                var total_write = Record.Value11;
                txtPerformanceWrite.Text = string.Format("{0} / {1}", total_write, current_write);

                var packets_received_total = Record.Value12;
                var packets_sent_total = Record.Value13;
                txtPerformancePackets.Text = string.Format("{0} / {1}", packets_received_total, packets_sent_total);

                var connections_total = Record.Value14;
                txtPerformanceConnections.Text = string.Format("{0} / {1}", connections_total, connections_current);
            }
            else
            {
                var dbIsStall = Record.Value13;
                var dbNumberReads = Record.Value1;
                var dbBytesRead = Record.Value2;
                var dbNumberWrites = Record.Value3;
                var dbBytesWritten = Record.Value4;
                var dbStartDate = Record.Value16;
                var dbFileCount = Record.Value15;

                var logIsStall = Record.Value14;
                var logNumberReads = Record.Value7;
                var logBytesRead = Record.Value8;
                var logNumberWrites = Record.Value9;
                var logBytesWritten = Record.Value10;

                txtPerformanceCPU.Text = dbStartDate.ToString();
                txtPerformanceIO.Text = string.Format("{0} / {1}", dbIsStall, logIsStall);
                ResetPerformanceIO();
                if (dbIsStall >= QueryEngine.DBStallThreshold
                    || logIsStall >= QueryEngine.DBStallThreshold)
                {
                    txtPerformanceIO.BackColor = Color.Red;
                    ttInfo.SetToolTip(txtPerformanceIO, "Potential performance bottleneck due to hard disk IO delay, check using database analysis.");
                    llPerformanceIO.Enabled = true;
                }
                txtPerformanceRead.Text = string.Format("{0} / {1}", dbNumberReads, Utils.FormatSize(dbBytesRead));
                txtPerformanceWrite.Text = string.Format("{0} / {1}", dbNumberWrites, Utils.FormatSize(dbBytesWritten));
                txtPerformancePackets.Text = string.Format("{0} / {1}", logNumberReads, Utils.FormatSize(logBytesRead));
                txtPerformanceConnections.Text = string.Format("{0} / {1}", logNumberWrites, Utils.FormatSize(logBytesWritten));
            }
            if (Record.Value16 > DateTime.MinValue)
                dtpPerformanceStartDate.Value = Record.Value16;
            isLoading = false;
        }

        private void OnPerformanceIOLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Monitor.Instance.ShowAnalysis();
        }

        internal void GetPerformanceData()
        {
            engine.CheckPerformance();
        }

        private void OnPopDockClick(object sender, EventArgs e)
        {
            SetPopDock();
        }

        internal void SetPopDock()
        {
            if (isDocked)
            {
                var dlg = new PerformanceDialog { Name = Title, Text = Title };
                dlg.Controls.Add(this);
                dlg.Show();
                Monitor.Instance.RemoveCurrentTab();
                cmdPopDock.Text = "Dock";
                dlg.BringToFront();
            }
            else
            {
                var parent = this.Parent as PerformanceDialog;
                Monitor.Instance.AddPerformance(this);
                parent.Close();
                cmdPopDock.Text = "Popup";
            }
            isDocked = !isDocked;
        }

        private void OnShowPerformanceHistoryCheckedChanged(object sender, EventArgs e)
        {
            var isChecked = chkShowPerformanceHistory.Checked;
            ResetPerformance(false);
            cboPerformanceViewTypes.Enabled = isChecked;
            dtpPerformanceStartDate.Enabled = isChecked;
            cmdPerformancePrevious.Enabled = isChecked;
            cmdPerformanceNext.Enabled = isChecked;
            var area = chPerformance.ChartAreas[0];
            if (isChecked)
            {
                area.AxisX.MajorGrid.Enabled = false;
                area.AxisX.LabelStyle.Enabled = false;
                area.CursorX.AxisType = AxisType.Primary;
                area.InnerPlotPosition.Auto = true;
                //pnlCurrentPoint.Visible = true;
                ShowHistory();
            }
            else
            {
                area.AxisX.MajorGrid.Enabled = true;
                area.AxisX.LabelStyle.Enabled = true;
                area.InnerPlotPosition.Auto = false;
                //pnlCurrentPoint.Visible = false;
            }
        }

        private void OnAutoPerformanceCheckedChanged(object sender, EventArgs e)
        {
            if (!isLoading)
            {
                if (chkAutoPerformance.Checked)
                    Settings.Instance.AddPerformanceItem(server, IsServer);
                else
                    Settings.Instance.RemovePerformanceItem(server, IsServer);
            }
        }

        private void ShowHistory()
        {
            if (server != null)
            {
                ResetPerformance(false);
                currentRecords = History.GetRecords(server, IsServer, (DateTypes)cboPerformanceViewTypes.SelectedItem, dtpPerformanceStartDate.Value);
                System.Diagnostics.Debug.WriteLine("count:" + currentRecords.Count);
                if (currentRecords.Count > 0)
                {
                    var min = currentRecords.Min(r => Convert.ToDateTime(r.Date));
                    var max = currentRecords.Max(r => Convert.ToDateTime(r.Date));
                    currentRecords.ForEach(r =>
                        {
                            r.Value16 = Convert.ToDateTime(r.Date);
                            AddPerformanceHistoryRecord(r, r.Value16, min, max);
                        });
                }
            }
        }

        private void OnPerformanceStartDateValueChanged(object sender, EventArgs e)
        {
            if (!isLoading)
                ShowHistory();
        }

        private void OnPerformancePreviousClick(object sender, EventArgs e)
        {
            MoveLeft();
        }

        private void MoveLeft()
        {
            dtpPerformanceStartDate.Value = dtpPerformanceStartDate.Value.AddDays(-1);
            //if (HasData && lastPointIndex > 0)
            //{
            //    lastPointIndex--;
            //    SetPerformanceInfo();
            //}
        }

        private void OnPerformanceNextClick(object sender, EventArgs e)
        {
            MoveRight();
        }

        private void MoveRight()
        {
            dtpPerformanceStartDate.Value = dtpPerformanceStartDate.Value.AddDays(1);
            //if (HasData && lastPointIndex < chPerformance.Series[0].Points.Count - 1)
            //{
            //    lastPointIndex++;
            //    SetPerformanceInfo();
            //}
        }

        private void SetPerformanceInfo()
        {
            if (lastPointIndex < currentRecords.Count)
                SetPerformanceInfo(currentRecords[lastPointIndex]);
        }

        private void OnPerformanceMouseDown(object sender, MouseEventArgs e)
        {
            //isMouseMoving = true;
        }

        private void OnPerformanceMouseUp(object sender, MouseEventArgs e)
        {
            //isMouseMoving = false;
            //SetPerformanceInfo(e);
        }

        private void OnPerformanceMouseMove(object sender, MouseEventArgs e)
        {
            //if (isMouseMoving)
            //{
            //    SetPerformanceInfo(e);
            //}
        }

        private bool HasData
        {
            get { return chPerformance.Series.Count > 0 && chPerformance.Series[0].Points.Count > 0; }
        }

        //private void SetPerformanceInfo(MouseEventArgs e)
        //{
        //    if (HasData)
        //    {
        //        var series = chPerformance.Series[0];
        //        var pixelPerPoint = (chPerformance.Width - chPerformance.ChartAreas[0].Position.Width) / series.Points.Count;
        //        var index = Convert.ToInt32((e.X - chPerformance.ChartAreas[0].Position.Width) / pixelPerPoint);
        //        if (lastPointIndex != index && index >= 0 && index < series.Points.Count)
        //        {
        //            lastPointIndex = index;
        //            //pnlCurrentPoint.Location = new Point(e.X, 0);
        //            SetPerformanceInfo();
        //        }
        //    }
        //}

        private void OnPerformanceMouseLeave(object sender, EventArgs e)
        {
            //isMouseMoving = false;
        }

        private void OnPerformanceKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    MoveLeft();
                    break;
                case Keys.Right:
                    MoveRight();
                    break;
                default:
                    break;
            }
        }

        private void OnPerformanceViewTypesSelectedIndexChanged(object sender, EventArgs e)
        {
            ShowHistory();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.KeyCode);
        }

    }
}