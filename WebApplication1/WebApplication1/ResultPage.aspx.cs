using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApplication1
{
    public partial class ResultPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!PreviousPage.IsValid)
            {
                LabelResult.Text = "Error in previous page";
                return;
            }
            try
            {
                DropDownList DropDownListEvent = (DropDownList)PreviousPage.FindControl("DropDownListEvent");
                string selectedEvent = DropDownListEvent.SelectedValue;
                string firstName = ((TextBox)PreviousPage.FindControl("TextFirstName")).Text;
                LabelResult.Text = string.Format("{0} selected {1}", firstName, selectedEvent);
            }
            catch (Exception ex)
            {
                LabelResult.Text = ex.Message;
            }
        }
    }
}