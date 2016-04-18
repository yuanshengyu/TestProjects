using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Xnlab.SQLMon
{
    public enum ObjectModes
    {
        None,
        Databases,
        Objects,
        Table,
        SP,
        View,
        Function,
        Assembly,
        Trigger,
        Job,
        Server,
        Index
    }

    internal enum WorkModes
    {
        Summary = 0,
        Objects = 1,
        Activities = 2,
        Performance = 3,
        Analysis = 4,
        Alerts = 5,
        Histories = 6,
        Options = 7,
        Query = 8,
        TableData = 9,
        UserPerformance = 10
    }

    class Utils
    {
        internal const int EmptyIndex = -1;
        internal const string FileExtenionSQL = ".sql";
        internal const string SizeKB = "KB";
        internal const string SizeMB = "MB";
        internal const string SizeGB = "GB";
        internal const int Size1K = 1024;
        internal const string TimeMS = "ms";
        internal const string MultiCommentStart = "/*";
        internal const string SingleCommentStart = "--";

        internal static T CloneObject<T>(T ObjectInstance)
        {
            BinaryFormatter bFormatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            bFormatter.Serialize(stream, ObjectInstance);
            stream.Seek(0, SeekOrigin.Begin);
            return (T)bFormatter.Deserialize(stream);
        }

        internal static void SetDragDropContent(TextBox Editor, DragEventArgs e)
        {
            var result = GetDragDropContent(e);
            if (!string.IsNullOrEmpty(result))
                Editor.Text = result;
        }

        internal static string GetDragDropContent(DragEventArgs e)
        {
            string result = null;
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
                result = e.Data.GetData(DataFormats.StringFormat).ToString();
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                result = System.IO.File.ReadAllText(files.First());
            }
            return result;
        }

        internal static void HandleContentDragEnter(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat)
                || e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        internal static string FormatSize(long Size)
        {
            if (Size > Size1K * Size1K * Size1K)
                return Convert.ToInt32(Size / Size1K / Size1K / Size1K) + " " + SizeGB;
            else if (Size > Size1K * Size1K)
                return Convert.ToInt32(Size / Size1K / Size1K) + " " + SizeMB;
            else if (Size > Size1K)
                return Convert.ToInt32(Size / Size1K) + " " + SizeKB;
            else
                return Size + " B";
        }

        internal static void SetTextBoxStyle(ICSharpCode.TextEditor.TextEditorControl Editor)
        {
            Editor.ShowEOLMarkers = false;
            Editor.ShowHRuler = false;
            Editor.ShowInvalidLines = false;
            Editor.ShowSpaces = false;
            Editor.ShowTabs = false;
            Editor.ShowMatchingBracket = true;
            Editor.AllowCaretBeyondEOL = false;
            Editor.ShowVRuler = false;
            Editor.ImeMode = ImeMode.On;
            Editor.SetHighlighting("TSQL");
        }

        internal static void SelectText(ICSharpCode.TextEditor.TextEditorControl Editor, string Text)
        {
            int offset = Editor.Text.IndexOf(Text);
            int endOffset = offset + Text.Length;
            Editor.ActiveTextAreaControl.TextArea.Caret.Position = Editor.ActiveTextAreaControl.TextArea.Document.OffsetToPosition(endOffset);
            Editor.ActiveTextAreaControl.TextArea.SelectionManager.ClearSelection();
            ICSharpCode.TextEditor.Document.IDocument document = Editor.ActiveTextAreaControl.TextArea.Document;
            ICSharpCode.TextEditor.Document.DefaultSelection selection = new ICSharpCode.TextEditor.Document.DefaultSelection(document, document.OffsetToPosition(offset), document.OffsetToPosition(endOffset));
            Editor.ActiveTextAreaControl.TextArea.SelectionManager.SetSelection(selection);
        }
    }
}
