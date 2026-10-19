using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace WAPP_EduNexus_App
{
    public partial class Site2 : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Username"] == null || Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                Response.Redirect("~/CommonPage/LoginPage.aspx");
                return;
            }

            lblAdminName.Text = Session["Username"].ToString();

            
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
            con.Open();

            SqlDataAdapter da = new SqlDataAdapter("SELECT ProfilePicture FROM Users WHERE Username = @username", con);
            da.SelectCommand.Parameters.AddWithValue("@username", Session["Username"].ToString());
            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();

           
            if (dt.Rows.Count > 0 && dt.Rows[0][0].ToString() != "")
            {
                AdminProfile.ImageUrl = dt.Rows[0][0].ToString() + "?v=" ;
            }
            else
            {
                AdminProfile.ImageUrl = "~/CommonPage/default-avatar.jpg";
            }
        }
    }
    }
