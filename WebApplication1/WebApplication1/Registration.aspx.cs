using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApplication1
{
    public partial class Registration : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void ButtonSubmit_Click(object sender, EventArgs e)
        {
            string str1 = DropDownListEvent.SelectedValue;
            string str2 = TextFirstName.Text;
            string str3 = TextLastName.Text;
            string str4 = TextEmail.Text;
            LabelResult.Text = string.Format("{0} {1} selected the event {2}", str2, str3, str1);
        }
    }
}