using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data.SqlClient;

namespace WAPP_EduNexus_App
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            // Check the input first
            if (txtUsername.Text == "" || txtPassword.Text == "")
            {
                errorMsg.Visible = true;
                errorMsg.Text = "Please enter your username and password.";
                return;
            }

            try
            {
                SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["UsersEduNexusDB"].ConnectionString);
                con.Open();

                // Find the user by username
                string query = "SELECT PasswordHash, Role FROM Users WHERE Username = @username";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@username", txtUsername.Text);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    string savedHash = reader["PasswordHash"].ToString();
                    string role = reader["Role"].ToString();

                    // Hash what the user typed and compare it with the saved hash
                    if (savedHash == PasswordHashing.Hash(txtPassword.Text))
                    {
                        // Remember who logged in
                        Session["Username"] = txtUsername.Text;
                        Session["Role"] = role;

                        reader.Close();
                        con.Close();

                        // Admin goes to the admin page, everyone else is a student
                        if (role == "Admin")
                        {
                            Response.Redirect("AdminPage.aspx");
                        }
                        else
                        {
                            Response.Redirect("HomePage.aspx");
                        }
                    }
                    else
                    {
                        errorMsg.Visible = true;
                        errorMsg.Text = "Wrong username or password.";
                    }
                }
                else
                {
                    errorMsg.Visible = true;
                    errorMsg.Text = "Wrong username or password.";
                }

                reader.Close();
                con.Close();
            }
            catch (Exception ex)
            {
                errorMsg.Visible = true;
                errorMsg.Text = "An error occurred while logging in.";
            }
        }
    }
}