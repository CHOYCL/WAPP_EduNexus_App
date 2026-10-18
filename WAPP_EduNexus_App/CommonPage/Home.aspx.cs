using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP_EduNexus_App.CommonPage
{
    public partial class Home : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Username"] != null)
            {
                getStartedLink.HRef = "~/LearningResourcePg/MyLearningPage.aspx";
                getStartedLink.InnerText = "Go to My Learning";
                LoggedUserMessage.InnerText = "Welcome back " + Session["Username"].ToString() + "! Pick up where you left off.";
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            // Take what the user typed, without spaces at the start or end
            string search = txtSearch.Text.Trim();

            if (search == "")
            {
                // Nothing typed: just show all courses
                Response.Redirect("~/LearningResourcePg/BrowseCourse.aspx");
            }
            else
            {
                // Send the word to the course page
                Response.Redirect("~/LearningResourcePg/BrowseCourse.aspx?search=" + Server.UrlEncode(search));
            }
        }

    }
}