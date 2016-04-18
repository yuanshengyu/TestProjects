using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Xnlab.SQLMon
{
    internal class DisposableState : IDisposable
    {
        private Control win;
        private ToolStripItem[] elements;

        internal DisposableState(Control Win, ToolStripItem[] Elements)
        {
            win = Win;
            elements = Elements;
            SetState(false);
        }

        private void SetState(bool State)
        {
            if (win.IsHandleCreated)
                win.Invoke(() =>
                    {
                        win.Cursor = State ? Cursors.Arrow : Cursors.WaitCursor;
                        if (elements != null)
                            elements.ToList().ForEach((o) => { o.Enabled = State; o.Invalidate(); });
                    }
                 );
        }

        public void Dispose()
        {
            SetState(true);
        }
    }
}
