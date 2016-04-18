using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Xnlab.SQLMon
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(OnApplicationThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnCurrentDomainUnhandledException);
            Application.Run(new Monitor());
        }

        private static void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception);
        }

        private static void OnApplicationThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            HandleException(e.Exception);
        }

        private static void HandleException(Exception e)
        {
            MessageBox.Show(e.Message, Settings.Title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
    }
}
