using System;
using System.Configuration;
using System.Data.SqlClient;

namespace WAPP_EduNexus_App.CommonPage
{
    public partial class RegisterPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            // Check the input first
            if (txtUsername.Text == "" || txtEmail.Text == "" || txtPassword.Text == "" || txtConfirm.Text == "")
            {
                errorMsg.Visible = true;
                errorMsg.Text = "Please fill in all fields.";
                return;
            }

            if (txtPassword.Text != txtConfirm.Text)
            {
                errorMsg.Visible = true;
                errorMsg.Text = "Passwords do not match.";
                return;
            }

            try
            {
                SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["UsersEduNexusDB"].ConnectionString);
                con.Open();

                // Check if the username or email is already used
                string query = "SELECT COUNT (*) FROM Users WHERE Username = @username OR Email = @email";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@username", txtUsername.Text);
                cmd.Parameters.AddWithValue("@email", txtEmail.Text);
                int check = Convert.ToInt32(cmd.ExecuteScalar().ToString());

                if (check > 0)
                {
                    errorMsg.Visible = true;
                    errorMsg.Text = "Username or email has been taken!";
                }
                else
                {
                    // Save the new student
                    string query1 = "INSERT INTO Users (Username, Email, PasswordHash, Role) VALUES (@username, @email, @passwordHash, @role)";

                    SqlCommand cmd1 = new SqlCommand(query1, con);

                    cmd1.Parameters.AddWithValue("@username", txtUsername.Text);
                    cmd1.Parameters.AddWithValue("@email", txtEmail.Text);
                    cmd1.Parameters.AddWithValue("@passwordHash", PasswordHashing.Hash(txtPassword.Text));
                    cmd1.Parameters.AddWithValue("@role", "Student");

                    cmd1.ExecuteNonQuery();
                    con.Close();
                    Response.Redirect("LoginPage.aspx");
                }
            }
            catch (Exception ex)
            {
                errorMsg.Visible = true;
                errorMsg.Text = "An error occurred while registering the member.";
            }
        }
    }
}