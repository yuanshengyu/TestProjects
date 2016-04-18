using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Xnlab.SQLMon
{
    public partial class ConnectionDialog : BaseDialog
    {
        public ConnectionDialog()
        {
            InitializeComponent();

            Enum.GetValues(typeof(AuthTypes)).Cast<AuthTypes>().ForEach((s) => cboAuthTypes.Items.Add(s));
            cboAuthTypes.SelectedIndex = 0;
        }

        public ConnectionDialog(ServerInfo Info)
            : this()
        {
            if (Info != null)
            {
                AuthType = Info.AuthType;
                Server = Info.Server;
                UserName = Info.User;
                Password = Info.Password;
                AuthType = Info.AuthType;
            }
        }

        public AuthTypes AuthType
        {
            get { return (AuthTypes)cboAuthTypes.SelectedItem; }
            set { cboAuthTypes.SelectedItem = value; }
        }

        public string Server
        {
            get { return cboServers.Text; }
            set { cboServers.Text = value; }
        }

        public string UserName
        {
            get { return txtUserName.Text; }
            set { txtUserName.Text = value; }
        }

        public string Password
        {
            get { return txtPassword.Text; }
            set { txtPassword.Text = value; }
        }

        private ServerInfo GetServerInfo
        {
            get
            {
                return new ServerInfo { AuthType = this.AuthType, Server = this.Server, User = this.UserName, Password = this.Password, Database = "master" };
            }
        }

        private void OnTestConnectionClick(object sender, EventArgs e)
        {
            if (IsSQLServer2005OrAbove())
                Monitor.Instance.ShowMessage("Connection is successful.");
        }

        private bool IsSQLServer2005OrAbove()
        {
            try
            {
                var version = SQLHelper.ExecuteScalar("SELECT SERVERPROPERTY('ProductVersion')", GetServerInfo);
                var value = version.ToString();
                var major = value.Split('.')[0];
                var is2005OrAbove = Convert.ToInt32(major) >= 9;
                if (!is2005OrAbove)
                    Monitor.Instance.ShowMessage(string.Format("Current version {0}, only SQL Server 2005 or above is supported.", version));
                return is2005OrAbove;
            }
            catch (Exception ex)
            {
                Monitor.Instance.ShowMessage(ex.Message);
                return false;
            }
        }

        private void OnSaveClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Server))
            {
                if (!string.IsNullOrEmpty(UserName) || AuthType == AuthTypes.Windows)
                {
                    if (IsSQLServer2005OrAbove())
                        this.DialogResult = DialogResult.OK;
                }
                else
                    epHint.SetError(txtUserName, "Please input user name.");
            }
            else
                epHint.SetError(cboServers, "Please input server.");
        }

        private void OnCancelClick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OnServersDropDown(object sender, EventArgs e)
        {
            if (cboServers.Items.Count == 0)
            {
                cboServers.Items.Clear();
                cboServers.Items.AddRange(Settings.Instance.Servers.Select((p) => p.Server).ToArray());
            }
        }

        private void OnAuthTypesSelectedIndexChanged(object sender, EventArgs e)
        {
            bool enable = (AuthTypes)cboAuthTypes.SelectedItem == AuthTypes.SQLServer;
            txtUserName.Enabled = enable;
            txtPassword.Enabled = enable;
        }
    }
}
