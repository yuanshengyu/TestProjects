using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace Xnlab.SQLMon
{
    class SQLHelper
    {
        internal static SqlConnection CreateNewConnection(ServerInfo Info)
        {
            var builder = new SqlConnectionStringBuilder();
            builder.ApplicationName = Settings.Title;
            builder.IntegratedSecurity = Info.AuthType == AuthTypes.Windows;
            builder.DataSource = Info.Server;
            builder.UserID = Info.User;
            builder.Password = Info.Password;
            builder.InitialCatalog = Info.Database ?? string.Empty;
            builder.ConnectTimeout = Settings.Instance.ConnectionTimeout;
            return new SqlConnection(builder.ConnectionString);
        }

        internal static DataSet QuerySet(string SQL, ServerInfo Info)
        {
            string message;
            return QuerySet(SQL, Info, out message);
        }

        internal static DataSet QuerySet(string SQL, ServerInfo Info, out string Message)
        {
            using (SqlConnection connection = SQLHelper.CreateNewConnection(Info))
            {
                var result = new StringBuilder();
                connection.InfoMessage += (s, e) => { result.AppendLine(e.Message); };
                var command = new SqlCommand(SQL, connection);
                command.StatementCompleted += (s, e) => { result.AppendLine(string.Format("{0} row(s) affected.", e.RecordCount)); };
                var adapter = new SqlDataAdapter(command);
                var data = new DataSet();
                //adapter.FillSchema(data, SchemaType.Mapped);
                adapter.Fill(data);
                connection.Close();
                Message = result.ToString();
                return data;
            }
        }

        internal static DataTable Query(string SQL, ServerInfo Info)
        {
            DataSet data = QuerySet(SQL, Info);
            if (data != null && data.Tables.Count > 0)
                return data.Tables[0];
            else
                return null;
        }

        internal static string ExecuteNonQuery(string SQL, ServerInfo Server)
        {
            using (SqlConnection connection = SQLHelper.CreateNewConnection(Server))
            {
                var result = new StringBuilder();
                connection.InfoMessage += (s, e) => { result.AppendLine(e.Message); };
                var command = new SqlCommand(SQL, connection);
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
                return result.ToString();
            }
        }

        internal static object ExecuteScalar(string SQL, ServerInfo Server)
        {
            object result;
            using (SqlConnection connection = SQLHelper.CreateNewConnection(Server))
            {
                var command = new SqlCommand(SQL, connection);
                connection.Open();
                result = command.ExecuteScalar();
                connection.Close();
            }
            return result;
        }

    }
}
