using System;
using System.Web.UI;

namespace WAPP_EduNexus_App
{
    public partial class MyDiscussions : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Username"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/CommonPage/LoginPage.aspx");
                return;
            }

            if (Session["Role"].ToString() == "Admin")
            {
                Response.Redirect("~/Admin/AdminManageDiscussion.aspx");
            }
        }
    }
}
