using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApplication1
{
    public partial class UpdatePanelDemo : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button_Click(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            string str = now.ToLongTimeString();
            Label1.Text = str;
            Label2.Text = str;
        }
    }
}