using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP_EduNexus_App.Admin
{
    public partial class AdminHome : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Only admins can use this page
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                Response.Redirect("~/CommonPage/Home.aspx");
                return;
            }

            // Load the numbers only the first time the page opens
            if (!Page.IsPostBack)
            {
                lblWelcome.Text = "Welcome back, " + Session["Username"].ToString() + ".";

                SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
                con.Open();

                // Count all users
                SqlCommand cmdTotal = new SqlCommand("SELECT COUNT (*) FROM Users", con);
                int total = Convert.ToInt32(cmdTotal.ExecuteScalar().ToString());

                // Count the admins
                SqlCommand cmdAdmins = new SqlCommand("SELECT COUNT (*) FROM Users WHERE Role = @role", con);
                cmdAdmins.Parameters.AddWithValue("@role", "Admin");
                int admins = Convert.ToInt32(cmdAdmins.ExecuteScalar().ToString());

                lblTotal.Text = total.ToString();
                lblAdmins.Text = admins.ToString();
                lblStudents.Text = (total - admins).ToString();

                // The 5 newest users
                SqlDataAdapter da = new SqlDataAdapter("SELECT TOP 5 Username, Email, Role FROM Users ORDER BY Id DESC", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                con.Close();

                gvNewUsers.DataSource = dt;
                gvNewUsers.DataBind();
            }
        }
    }
}