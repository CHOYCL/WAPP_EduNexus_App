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
    public partial class AdminManageUser : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                Response.Redirect("~/CommonPage/Home.aspx");
                return;
            }

            // Load the users only the first time the page opens
            if (!Page.IsPostBack)
            {
                LoadUsers();
            }
        }

        // Get all users from the database and show them as cards
        private void LoadUsers()
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
            con.Open();

            SqlDataAdapter da = new SqlDataAdapter("SELECT Id, Username, Email, PhoneNumber, Role, ProfilePicture FROM Users ORDER BY Role, Username", con);
            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();

            rptUsers.DataSource = dt;
            rptUsers.DataBind();

            lblCount.Text = "Total users: " + dt.Rows.Count;
        }

        // Use the default picture if the user has not uploaded one
        protected string GetUserPic(object pic)
        {
            if (pic == null || pic.ToString() == "")
            {
                return "~/CommonPage/default-avatar.jpg";
            }

            return pic.ToString();
        }

        protected void rptUsers_ItemCommand(object source, RepeaterCommandEventArgs e)
        {

            int userId = Convert.ToInt32(e.CommandArgument);

            try
            {
                SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
                con.Open();

                // Find the username and role of this user
                SqlDataAdapter da = new SqlDataAdapter("SELECT Username, Role FROM Users WHERE Id = @id", con);
                da.SelectCommand.Parameters.AddWithValue("@id", userId);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    string username = dt.Rows[0]["Username"].ToString();
                    string role = dt.Rows[0]["Role"].ToString();

                    if (e.CommandName == "DeleteUser")
                    {
                        if (role == "Admin")
                        {
                            // Admin accounts cannot be deleted
                            lblMessage.Visible = true;
                            lblMessage.ForeColor = System.Drawing.Color.Red;
                            lblMessage.Text = "Admin accounts cannot be deleted.";
                        }
                        else
                        {
                            SqlCommand cmdDelete = new SqlCommand("DELETE FROM Users WHERE Id = @id", con);
                            cmdDelete.Parameters.AddWithValue("@id", userId);
                            cmdDelete.ExecuteNonQuery();

                            lblMessage.Visible = true;
                            lblMessage.ForeColor = System.Drawing.Color.Green;
                            lblMessage.Text = "User " + username + " has been deleted.";
                        }
                    }
                    else if (e.CommandName == "PromoteUser")
                    {
                        SqlCommand cmdPromote = new SqlCommand("UPDATE Users SET Role = @role WHERE Id = @id", con);
                        cmdPromote.Parameters.AddWithValue("@role", "Admin");
                        cmdPromote.Parameters.AddWithValue("@id", userId);
                        cmdPromote.ExecuteNonQuery();

                        lblMessage.Visible = true;
                        lblMessage.ForeColor = System.Drawing.Color.Green;
                        lblMessage.Text = "User " + username + " is now an admin.";
                    }
                }

                con.Close();
            }
            catch (Exception ex)
            {
                lblMessage.Visible = true;
                lblMessage.ForeColor = System.Drawing.Color.Red;
                lblMessage.Text = "An error occurred. " + ex.Message;
            }

            // Show the updated list
            LoadUsers();
        }
    }
}
