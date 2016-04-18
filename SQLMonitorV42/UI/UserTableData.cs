using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Threading;

namespace Xnlab.SQLMon
{
    public partial class UserTableData : UserControl, ICancelable
    {
        private string table;
        private ServerInfo server;
        private bool isRunning = false;
        private Thread thread = null;
        private bool hasPrimaryKey = false;
        private string primaryKey = null;

        public UserTableData()
        {
            InitializeComponent();
        }

        public UserTableData(ServerInfo Server, string Table)
            : this()
        {
            rtbSQL.Font = Monitor.Instance.SetFont();
            server = Server;
            table = Table;
            var sql = "SELECT TOP 100 * FROM " + table;
            Utils.SetTextBoxStyle(rtbSQL);
            rtbSQL.Text = sql;
            Execute();
        }

        ~UserTableData()
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

        public string Key
        {
            get { return server.Server + "." + server.Database + "." + table; }
        }

        public void Execute()
        {
            if (!isRunning)
            {
                using (new DisposableState(this, Monitor.Instance.Commands))
                {
                    thread = new Thread(new ParameterizedThreadStart(StartQuery));
                    thread.Start(rtbSQL.Text);
                }
            }
            else
            {
                if (thread != null)
                    thread.Abort();
                isRunning = false;
                SetCommand(false);
            }
        }

        private void StartQuery(object State)
        {
            try
            {
                SetCommand(true);
                var data = SQLHelper.Query((string)State, Monitor.Instance.CurrentServerInfo);
                if (data != null)
                    data.TableName = table;

                string schemaName;
                var tableName = QueryEngine.ParseObjectName(table, out schemaName);
                var sql = string.Format(@"SELECT  COL_NAME(ic.OBJECT_ID,ic.column_id)
FROM    sys.indexes AS i INNER JOIN 
        sys.index_columns AS ic ON  i.OBJECT_ID = ic.OBJECT_ID
                                AND i.index_id = ic.index_id
WHERE OBJECT_NAME(ic.OBJECT_ID) = '{0}' AND i.is_primary_key = 1", tableName);
                var result = SQLHelper.ExecuteScalar(sql, Monitor.Instance.CurrentServerInfo);
                primaryKey = result != DBNull.Value ? Convert.ToString(result) : string.Empty;
                hasPrimaryKey = !string.IsNullOrEmpty(primaryKey);
                this.Invoke(() =>
                    {
                        dgvData.DataSource = data;
                        dgvData.ReadOnly = !hasPrimaryKey;
                    });
            }
            catch (Exception ex)
            {
                ShowMessage(ex is ThreadAbortException ? "Query cancelled." : ex.Message);
            }
            finally
            {
                SetCommand(false);
            }
        }

        private void OnDataGridRowLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (hasPrimaryKey)
            {
                var data = dgvData.DataSource as DataTable;
                if (data != null)
                    UpdateQueryData(data);
            }
        }

        private void StartUpdate(DataTable UserData)
        {
            try
            {
                SetCommand(true);
                //dgvData.AllowUserToAddRows = false;
                var userData = (DataTable)UserData;
                userData = userData.GetChanges();
                if (userData != null)
                {
                    using (SqlConnection connection = SQLHelper.CreateNewConnection(Monitor.Instance.CurrentServerInfo))
                    {
                        connection.Open();
                        using (SqlTransaction transaction = connection.BeginTransaction())
                        {
                            string schemaName;
                            var tableName = QueryEngine.ParseObjectName(userData.TableName, out schemaName);
                            tableName = QueryEngine.GetObjectName(schemaName, tableName);
                            using (SqlCommand command = new SqlCommand("SELECT TOP 1 * FROM " + tableName, connection))
                            {
                                command.Transaction = transaction;
                                SqlDataAdapter adapter = new SqlDataAdapter(command);
                                SqlCommandBuilder builder = new SqlCommandBuilder(adapter);
                                adapter.InsertCommand = builder.GetInsertCommand();
                                adapter.DeleteCommand = builder.GetDeleteCommand();
                                //for (int i = adapter.DeleteCommand.Parameters.Count - 1; i >= 0; i--)
                                //{
                                //    if (adapter.DeleteCommand.Parameters[i].SourceColumn != primaryKey)
                                //        adapter.DeleteCommand.Parameters.RemoveAt(i);
                                //}
                                adapter.UpdateCommand = builder.GetUpdateCommand();
                                //for (int i = adapter.UpdateCommand.Parameters.Count - 1; i >= 0; i--)
                                //{
                                //    if (adapter.UpdateCommand.Parameters[i].SourceColumn != primaryKey)
                                //        adapter.UpdateCommand.Parameters.RemoveAt(i);
                                //}
                                adapter.Update(userData);
                                UserData.AcceptChanges();
                            }
                            transaction.Commit();
                        }
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message);
            }
            finally
            {
                SetCommand(false);
                //dgvData.AllowUserToAddRows = true;
            }
        }

        private void ShowMessage(string Message)
        {
            this.Invoke(() =>
                {
                    Monitor.Instance.ShowMessage(Message);
                });
        }

        private void UpdateQueryData(DataTable UserData)
        {
            using (new DisposableState(this, Monitor.Instance.Commands))
            {
                StartUpdate(UserData);
            }
        }

    }
}
