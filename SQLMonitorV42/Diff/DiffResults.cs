using System;
using System.Text;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using DifferenceEngine;

//source: http://www.codeproject.com/KB/recipes/diffengine.aspx

namespace Xnlab.SQLMon
{
    /// <summary>
    /// Summary description for Results.
    /// </summary>
    public class DiffResults : System.Windows.Forms.Form
    {
        private SplitContainer splitContainer1;
        private ComboBox cboSources;
        private Label lblSource;
        private ComboBox cboDestinations;
        private Label lblDestination;
        private bool isLoaded = false;
        private ICSharpCode.TextEditor.TextEditorControl rtbSourceScript;
        private ICSharpCode.TextEditor.TextEditorControl rtbDestinationScript;
        private Button cmdCompare;
        private int previousVersion = 0;
        private int currentVersion = 0;
        private const string DefaultCompare = "-- PUT YOUR SCRIPT HERE TO COMPARE";
        private List<int> differences = new List<int>();
        private int currentIndex = 0;
        private Panel pnlSearchCommands;
        private Button cmdDifferenceNext;
        private Button cmdDifferencePrevious;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public DiffResults()
        {
            InitializeComponent();

            this.Icon = Icon.FromHandle(Properties.Resources.History2.GetHicon());

            Utils.SetTextBoxStyle(rtbSourceScript);
            Utils.SetTextBoxStyle(rtbDestinationScript);

            //rtbSourceScript.CurrentLineColor = Color.Blue;
            //rtbDestinationScript.CurrentLineColor = Color.Blue;

        }

        public DiffResults(string SourceScript)
            : this()
        {
            currentVersion = Utils.EmptyIndex;
            previousVersion = Utils.EmptyIndex;
            cboSources.Visible = false;
            cboDestinations.Visible = false;
            isLoaded = true;
            TextDiff(SourceScript, false, DefaultCompare, false);
        }

        public DiffResults(int PreviousVersion, int CurrentVersion)
            : this()
        {
            previousVersion = PreviousVersion;

            if (previousVersion == Utils.EmptyIndex)
                cboSources.Visible = false;
            else
            {
                cboSources.DataSource = Monitor.Instance.GetObjectVersions();
                cboSources.SelectedValue = previousVersion;
            }

            currentVersion = CurrentVersion;
            if (currentVersion == Utils.EmptyIndex)
                cboDestinations.Visible = false;
            else
            {
                cboDestinations.DataSource = Monitor.Instance.GetObjectVersions();
                cboDestinations.SelectedValue = currentVersion;
            }
            isLoaded = true;
            ShowVersionDiff();
        }

        private void TextDiff(int PreviousVersion, int CurrentVersion)
        {
            string sourceScript = PreviousVersion != Utils.EmptyIndex ? GetVersionScript(PreviousVersion) : DefaultCompare;
            string destinationScript = CurrentVersion != Utils.EmptyIndex ? GetVersionScript(CurrentVersion) : string.Empty;

            TextDiff(sourceScript, false, destinationScript, false);
        }

        private string GetVersionScript(int Version)
        {
            if (Version == 0)
                return Monitor.Instance.GetObjectScriptTextEx();
            else
                return Monitor.Instance.GetObjectScriptVersionText(Version);
        }

        private void ShowDiff(DiffListText source, DiffListText destination, List<DiffResultSpan> DiffLines, double seconds)
        {
            currentIndex = 0;
            cmdDifferenceNext.Enabled = true;
            cmdDifferencePrevious.Enabled = true;
            differences = new List<int>();
            rtbSourceScript.BeginUpdate();
            rtbDestinationScript.BeginUpdate();
            List<KeyValuePair<string, Color>> sourceLines = new List<KeyValuePair<string, Color>>();
            List<KeyValuePair<string, Color>> destinationLines = new List<KeyValuePair<string, Color>>();
            int i;

            foreach (DiffResultSpan drs in DiffLines)
            {
                switch (drs.Status)
                {
                    case DiffResultSpanStatus.DeleteSource:
                        differences.Add(sourceLines.Count);
                        for (i = 0; i < drs.Length; i++)
                        {
                            sourceLines.Add(new KeyValuePair<string, Color>(((TextLine)source.GetByIndex(drs.SourceIndex + i)).Line, Color.Red));
                            destinationLines.Add(new KeyValuePair<string, Color>(new string(' ', ((TextLine)source.GetByIndex(drs.SourceIndex + i)).Line.Length), Color.Blue));
                        }
                        break;
                    case DiffResultSpanStatus.NoChange:
                        for (i = 0; i < drs.Length; i++)
                        {
                            sourceLines.Add(new KeyValuePair<string, Color>(((TextLine)source.GetByIndex(drs.SourceIndex + i)).Line, Color.Empty));
                            destinationLines.Add(new KeyValuePair<string, Color>(((TextLine)destination.GetByIndex(drs.DestIndex + i)).Line, Color.Empty));
                        }
                        break;
                    case DiffResultSpanStatus.AddDestination:
                        differences.Add(sourceLines.Count);
                        for (i = 0; i < drs.Length; i++)
                        {
                            sourceLines.Add(new KeyValuePair<string, Color>(new string(' ', ((TextLine)destination.GetByIndex(drs.DestIndex + i)).Line.Length), Color.Blue));
                            destinationLines.Add(new KeyValuePair<string, Color>(((TextLine)destination.GetByIndex(drs.DestIndex + i)).Line, Color.Blue));
                        }
                        break;
                    case DiffResultSpanStatus.Replace:
                        differences.Add(sourceLines.Count);
                        for (i = 0; i < drs.Length; i++)
                        {
                            sourceLines.Add(new KeyValuePair<string, Color>(((TextLine)source.GetByIndex(drs.SourceIndex + i)).Line, Color.Red));
                            destinationLines.Add(new KeyValuePair<string, Color>(((TextLine)destination.GetByIndex(drs.DestIndex + i)).Line, Color.Green));
                        }
                        break;
                }
            }

            FillScript(sourceLines, rtbSourceScript);
            FillScript(destinationLines, rtbDestinationScript);

            rtbSourceScript.EndUpdate();
            rtbDestinationScript.EndUpdate();

            this.Text = differences.Count + " differences.";
        }

        private void FillScript(List<KeyValuePair<string, Color>> Lines, ICSharpCode.TextEditor.TextEditorControl TextBox)
        {
            var script = new StringBuilder();
            foreach (var item in Lines)
            {
                script.AppendLine(item.Key);
            }
            TextBox.Text = script.ToString();
            for (int i = 0; i < Lines.Count; i++)
            {
                if (Lines[i].Value == Color.Blue
                    || Lines[i].Value == Color.Green
                    || Lines[i].Value == Color.Red)
                {
                    ICSharpCode.TextEditor.Document.TextMarker marker = new ICSharpCode.TextEditor.Document.TextMarker(TextBox.Document.LineSegmentCollection[i].Offset, TextBox.Document.LineSegmentCollection[i].Length, ICSharpCode.TextEditor.Document.TextMarkerType.SolidBlock, Color.White, Lines[i].Value);
                    TextBox.Document.MarkerStrategy.AddMarker(marker);
                }
            }
        }

        public void TextDiff(string Source, bool SourceIsFile, string Destination, bool DestinationIsFile)
        {
            using (new DisposableState(this, null))
            {
                DiffListText sLF = null;
                DiffListText dLF = null;
                try
                {
                    sLF = new DiffListText(Source, SourceIsFile);
                    dLF = new DiffListText(Destination, DestinationIsFile);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "File Error");
                    return;
                }

                try
                {
                    double time = 0;
                    DiffEngine de = new DiffEngine();
                    time = de.ProcessDiff(sLF, dLF, DiffEngineLevel.SlowPerfect);

                    List<DiffResultSpan> rep = de.DiffReport();
                    ShowDiff(sLF, dLF, rep, time);
                }
                catch (Exception ex)
                {
                    string tmp = string.Format("{0}{1}{1}***STACK***{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                    MessageBox.Show(tmp, "Compare Error");
                }
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.rtbSourceScript = new ICSharpCode.TextEditor.TextEditorControl();
            this.cboSources = new System.Windows.Forms.ComboBox();
            this.lblSource = new System.Windows.Forms.Label();
            this.cmdCompare = new System.Windows.Forms.Button();
            this.rtbDestinationScript = new ICSharpCode.TextEditor.TextEditorControl();
            this.cboDestinations = new System.Windows.Forms.ComboBox();
            this.lblDestination = new System.Windows.Forms.Label();
            this.pnlSearchCommands = new System.Windows.Forms.Panel();
            this.cmdDifferenceNext = new System.Windows.Forms.Button();
            this.cmdDifferencePrevious = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.pnlSearchCommands.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.rtbSourceScript);
            this.splitContainer1.Panel1.Controls.Add(this.cboSources);
            this.splitContainer1.Panel1.Controls.Add(this.lblSource);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.cmdCompare);
            this.splitContainer1.Panel2.Controls.Add(this.rtbDestinationScript);
            this.splitContainer1.Panel2.Controls.Add(this.cboDestinations);
            this.splitContainer1.Panel2.Controls.Add(this.lblDestination);
            this.splitContainer1.Size = new System.Drawing.Size(862, 531);
            this.splitContainer1.SplitterDistance = 425;
            this.splitContainer1.TabIndex = 3;
            // 
            // rtbSourceScript
            // 
            this.rtbSourceScript.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbSourceScript.AutoScroll = true;
            this.rtbSourceScript.AutoScrollMinSize = new System.Drawing.Size(25, 15);
            this.rtbSourceScript.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.rtbSourceScript.IsReadOnly = false;
            this.rtbSourceScript.Location = new System.Drawing.Point(6, 34);
            this.rtbSourceScript.Name = "rtbSourceScript";
            this.rtbSourceScript.Size = new System.Drawing.Size(409, 493);
            this.rtbSourceScript.TabIndex = 5;
            // 
            // cboSources
            // 
            this.cboSources.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboSources.DisplayMember = "Key";
            this.cboSources.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSources.FormattingEnabled = true;
            this.cboSources.Location = new System.Drawing.Point(90, 9);
            this.cboSources.Name = "cboSources";
            this.cboSources.Size = new System.Drawing.Size(325, 20);
            this.cboSources.TabIndex = 4;
            this.cboSources.ValueMember = "Value";
            this.cboSources.SelectedIndexChanged += new System.EventHandler(this.OnSourcesSelectedIndexChanged);
            // 
            // lblSource
            // 
            this.lblSource.AutoSize = true;
            this.lblSource.Location = new System.Drawing.Point(4, 11);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(47, 12);
            this.lblSource.TabIndex = 3;
            this.lblSource.Text = "&Source:";
            // 
            // cmdCompare
            // 
            this.cmdCompare.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCompare.Location = new System.Drawing.Point(347, 9);
            this.cmdCompare.Name = "cmdCompare";
            this.cmdCompare.Size = new System.Drawing.Size(78, 22);
            this.cmdCompare.TabIndex = 8;
            this.cmdCompare.Text = "&Compare";
            this.cmdCompare.UseVisualStyleBackColor = true;
            this.cmdCompare.Click += new System.EventHandler(this.OnCompareClick);
            // 
            // rtbDestinationScript
            // 
            this.rtbDestinationScript.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbDestinationScript.AutoScroll = true;
            this.rtbDestinationScript.AutoScrollMinSize = new System.Drawing.Size(25, 15);
            this.rtbDestinationScript.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.rtbDestinationScript.IsReadOnly = false;
            this.rtbDestinationScript.Location = new System.Drawing.Point(6, 34);
            this.rtbDestinationScript.Name = "rtbDestinationScript";
            this.rtbDestinationScript.Size = new System.Drawing.Size(424, 493);
            this.rtbDestinationScript.TabIndex = 7;
            // 
            // cboDestinations
            // 
            this.cboDestinations.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboDestinations.DisplayMember = "Key";
            this.cboDestinations.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDestinations.FormattingEnabled = true;
            this.cboDestinations.Location = new System.Drawing.Point(92, 9);
            this.cboDestinations.Name = "cboDestinations";
            this.cboDestinations.Size = new System.Drawing.Size(247, 20);
            this.cboDestinations.TabIndex = 6;
            this.cboDestinations.ValueMember = "Value";
            this.cboDestinations.SelectedIndexChanged += new System.EventHandler(this.OnDestinationsSelectedIndexChanged);
            // 
            // lblDestination
            // 
            this.lblDestination.AutoSize = true;
            this.lblDestination.Location = new System.Drawing.Point(6, 11);
            this.lblDestination.Name = "lblDestination";
            this.lblDestination.Size = new System.Drawing.Size(77, 12);
            this.lblDestination.TabIndex = 5;
            this.lblDestination.Text = "&Destination:";
            // 
            // pnlSearchCommands
            // 
            this.pnlSearchCommands.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlSearchCommands.Controls.Add(this.cmdDifferenceNext);
            this.pnlSearchCommands.Controls.Add(this.cmdDifferencePrevious);
            this.pnlSearchCommands.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlSearchCommands.Location = new System.Drawing.Point(0, 531);
            this.pnlSearchCommands.Name = "pnlSearchCommands";
            this.pnlSearchCommands.Size = new System.Drawing.Size(862, 29);
            this.pnlSearchCommands.TabIndex = 5;
            // 
            // cmdDifferenceNext
            // 
            this.cmdDifferenceNext.Location = new System.Drawing.Point(102, 5);
            this.cmdDifferenceNext.Name = "cmdDifferenceNext";
            this.cmdDifferenceNext.Size = new System.Drawing.Size(90, 21);
            this.cmdDifferenceNext.TabIndex = 4;
            this.cmdDifferenceNext.Text = "&Next";
            this.cmdDifferenceNext.UseVisualStyleBackColor = true;
            this.cmdDifferenceNext.Click += new System.EventHandler(this.OnDifferenceNextClick);
            // 
            // cmdDifferencePrevious
            // 
            this.cmdDifferencePrevious.Enabled = false;
            this.cmdDifferencePrevious.Location = new System.Drawing.Point(5, 4);
            this.cmdDifferencePrevious.Name = "cmdDifferencePrevious";
            this.cmdDifferencePrevious.Size = new System.Drawing.Size(90, 21);
            this.cmdDifferencePrevious.TabIndex = 3;
            this.cmdDifferencePrevious.Text = "&Previous";
            this.cmdDifferencePrevious.UseVisualStyleBackColor = true;
            this.cmdDifferencePrevious.Click += new System.EventHandler(this.OnDifferencePreviousClick);
            // 
            // DiffResults
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(862, 560);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.pnlSearchCommands);
            this.KeyPreview = true;
            this.MinimizeBox = false;
            this.Name = "DiffResults";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Diff Results";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnFormKeyDown);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.pnlSearchCommands.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private void OnSourcesSelectedIndexChanged(object sender, EventArgs e)
        {
            ShowVersionDiff();
        }

        private void ShowVersionDiff()
        {
            if (isLoaded)
                TextDiff(previousVersion != Utils.EmptyIndex && cboSources.SelectedValue != null ? Convert.ToInt32(cboSources.SelectedValue) : Utils.EmptyIndex
                    , currentVersion != Utils.EmptyIndex && cboDestinations.SelectedValue != null ? Convert.ToInt32(cboDestinations.SelectedValue) : Utils.EmptyIndex);
        }

        private void OnDestinationsSelectedIndexChanged(object sender, EventArgs e)
        {
            ShowVersionDiff();
        }

        //private void OnScriptVisibleRangeChanged(object sender, EventArgs e)
        //{
        //    var box = (sender as FastColoredTextBox);
        //    var vPos = box.VerticalScroll.Value;
        //    var curLine = box.Selection.Start.iLine;

        //    if (box == rtbSourceScript)
        //        UpdateScroll(rtbDestinationScript, vPos, curLine);
        //    else
        //        UpdateScroll(rtbSourceScript, vPos, curLine);
        //}

        //private void UpdateScroll(ICSharpCode.TextEditor.TextEditorControl tb, int vPos, int curLine)
        //{
        //    if (updating > 0)
        //        return;
        //    //
        //    BeginUpdate();
        //    //
        //    if (vPos <= tb.VerticalScroll.Maximum)
        //    {
        //        tb.VerticalScroll.Value = vPos;

        //        //some magic for scroll update (it's no my bug, it's Microsoft :)
        //        tb.AutoScrollMinSize -= new Size(1, 0);
        //        tb.AutoScrollMinSize += new Size(1, 0);
        //    }

        //    if (curLine < tb.LinesCount)
        //        tb.Selection = new Range(tb, 0, curLine, 0, curLine);
        //    //
        //    EndUpdate();
        //}

        //private void BeginUpdate()
        //{
        //    updating++;
        //}

        //private void EndUpdate()
        //{
        //    updating--;
        //}

        private void OnFormKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
                ShowDiff();
        }

        private void ShowDiff()
        {
            if (!cboSources.Visible || !cboDestinations.Visible)
                TextDiff(rtbSourceScript.Text, false, rtbDestinationScript.Text, false);
            else
                ShowVersionDiff();
        }

        private void OnCompareClick(object sender, EventArgs e)
        {
            ShowDiff();
        }

        private void OnDifferenceNextClick(object sender, EventArgs e)
        {
            SetDifferencePosition(true);
        }

        private void SetDifferencePosition(bool IsNext)
        {
            if (IsNext)
                currentIndex++;
            else
                currentIndex--;
            if (currentIndex >= differences.Count)
                currentIndex = differences.Count - 1;
            if (currentIndex < 0)
                currentIndex = 0;
            cmdDifferenceNext.Enabled = currentIndex < differences.Count - 1 && differences.Count > 0;
            cmdDifferencePrevious.Enabled = currentIndex > 0 && differences.Count > 0;
            if (currentIndex < differences.Count)
            {
                rtbSourceScript.ActiveTextAreaControl.TextArea.Caret.Position = new ICSharpCode.TextEditor.TextLocation(0, differences[currentIndex]);
                rtbSourceScript.ActiveTextAreaControl.TextArea.ScrollToCaret();
            }
        }

        private void OnDifferencePreviousClick(object sender, EventArgs e)
        {
            SetDifferencePosition(false);
        }
    }
}
