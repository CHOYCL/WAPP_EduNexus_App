using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP_EduNexus_App
{
    public partial class Site1 : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string currentPage = System.IO.Path.GetFileName(Page.AppRelativeVirtualPath);

            if (currentPage.Equals("BrowseCourse.aspx", StringComparison.OrdinalIgnoreCase))
            {
                navBrowse.Attributes["class"] = "nav-link active";
            }
            else if (currentPage.Equals("MyLearningPage.aspx", StringComparison.OrdinalIgnoreCase))
            {
                navLearning.Attributes["class"] = "nav-link active";
            }
            else if (currentPage.Equals("Home.aspx", StringComparison.OrdinalIgnoreCase))
            {
                navHome.Attributes["class"] = "nav-link active";
            }

            if (Session["Username"] != null)
            {
                //  show the username instead
                footerRegister.Visible = false;
                footerLogin.Visible = false;
                footerUser.Visible = true;
                footerUser.InnerText = Session["Username"].ToString();

                //  Sign Up buttons
                GuestBadge.Visible = false;
                UserBadge.Visible = true;
                lblHeaderName.Text = Session["Username"].ToString();

                // Get the user's profile picture from the database
                SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
                con.Open();

                SqlDataAdapter da = new SqlDataAdapter("SELECT ProfilePicture FROM Users WHERE Username = @username", con);
                da.SelectCommand.Parameters.AddWithValue("@username", Session["Username"].ToString());
                DataTable dt = new DataTable();
                da.Fill(dt);
                con.Close();

                // Use the default picture if the user has not uploaded one
                if (dt.Rows.Count > 0 && dt.Rows[0][0].ToString() != "")
                {
                    // ?v=... makes the browser load the newest picture after an upload
                    imgUser.ImageUrl = dt.Rows[0][0].ToString() + "?v=";
                }
                else
                {
                    imgUser.ImageUrl = "~/CommonPage/default-avatar.jpg";
                }
            }
        }
    }
}