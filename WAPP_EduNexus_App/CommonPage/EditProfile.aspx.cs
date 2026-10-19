using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP_EduNexus_App.CommonPage
{
    public partial class EditProfile : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Admins Profile is in another page, this is a just in case check.
            if (Session["Role"] != null && Session["Role"].ToString() == "Admin")
            {
                Response.Redirect("~/Admin/AdminEditProfile.aspx");
                return;
            }

            // Check if the user is logged in
            if (Session["Username"] != null)
            {
                // Only load profile data from the database on the initial page load
                if (!Page.IsPostBack)
                {
                    SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
                    con.Open();

                    SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Users WHERE Username = @username", con);
                    da.SelectCommand.Parameters.AddWithValue("@username", Session["Username"].ToString());
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    con.Close();

                    // Populate the labels with data from the first row returned
                    if (dt.Rows.Count > 0)
                    {
                        lblUsername.Text = dt.Rows[0][1].ToString();
                        lblEmail.Text = dt.Rows[0][2].ToString();
                        lblPhone.Text = dt.Rows[0][5].ToString().Trim();

                        // Use the default picture if the user has not uploaded one
                        if (dt.Rows[0][6].ToString() == "")
                        {
                            imgProfile.ImageUrl = "~/CommonPage/default-avatar.jpg";
                        }
                        else
                        {
                            imgProfile.ImageUrl = dt.Rows[0][6].ToString();
                        }
                    }
                }
            }
            else
            {
                // Redirect to homepage
                Response.Redirect("Home.aspx");
            }
        }

        // Log Out button
        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("Home.aspx");
        }

        // Edit button
        protected void btnEdit_Click(object sender, EventArgs e)
        {
            txtUsername.Text = lblUsername.Text;
            txtEmail.Text = lblEmail.Text;
            txtPhone.Text = lblPhone.Text;
            imgEdit.ImageUrl = imgProfile.ImageUrl;

            lblMessage.Visible = false;
            pnlView.Visible = false;
            pnlEdit.Visible = true;
        }

        // Cancel button: go back without saving
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            lblMessage.Visible = false;
            pnlEdit.Visible = false;
            pnlView.Visible = true;
        }

        // Save button
        protected void btnSave_Click(object sender, EventArgs e)
        {
            // Check the input first
            if (txtUsername.Text == "" || txtEmail.Text == "")
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Username and email cannot be empty.";
                return;
            }

            if (txtNewPassword.Text != txtConfirmPassword.Text)
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Passwords do not match.";
                return;
            }

            // Check the picture type only if the user chose a file
            string ext = "";
            if (UploadPic.HasFile)
            {
                ext = Path.GetExtension(UploadPic.FileName).ToLower();

                if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                {
                    lblMessage.Visible = true;
                    lblMessage.Text = "Only JPG or PNG pictures are allowed.";
                    return;
                }
            }

            try
            {
                SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
                con.Open();

                // Check if the new username or email belongs to another user
                int checkUsername = 0;

                if (txtUsername.Text != Session["Username"].ToString())
                {
                    string query = "SELECT COUNT (*) FROM Users WHERE Username = @newUsername";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@newUsername", txtUsername.Text);
                    checkUsername = Convert.ToInt32(cmd.ExecuteScalar().ToString());
                }

                // Check the email only if it was changed
                int checkEmail = 0;

                if (txtEmail.Text != lblEmail.Text)
                {
                    string queryEmail = "SELECT COUNT (*) FROM Users WHERE Email = @newEmail";
                    SqlCommand cmdEmail = new SqlCommand(queryEmail, con);
                    cmdEmail.Parameters.AddWithValue("@newEmail", txtEmail.Text);
                    checkEmail = Convert.ToInt32(cmdEmail.ExecuteScalar().ToString());
                }

                if (checkUsername > 0)
                {
                    lblMessage.Visible = true;
                    lblMessage.Text = "Username has been taken!";
                }
                else if (checkEmail > 0)
                {
                    lblMessage.Visible = true;
                    lblMessage.Text = "Email has been taken!";
                }
                else
                {
                    // Update username, email and phone
                    string query1 = "UPDATE Users SET Username = @newUsername, Email = @newEmail, PhoneNumber = @phoneNumber WHERE Username = @oldUsername";
                    SqlCommand cmd1 = new SqlCommand(query1, con);
                    cmd1.Parameters.AddWithValue("@newUsername", txtUsername.Text);
                    cmd1.Parameters.AddWithValue("@newEmail", txtEmail.Text);
                    cmd1.Parameters.AddWithValue("@phoneNumber", txtPhone.Text.Trim());
                    cmd1.Parameters.AddWithValue("@oldUsername", Session["Username"].ToString());
                    cmd1.ExecuteNonQuery();

                    // The username may have changed, so remember the new one
                    Session["Username"] = txtUsername.Text;

                    // Update the password only if the user typed a new one
                    if (txtNewPassword.Text != "")
                    {
                        string query2 = "UPDATE Users SET PasswordHash = @passwordHash WHERE Username = @username";
                        SqlCommand cmd2 = new SqlCommand(query2, con);
                        cmd2.Parameters.AddWithValue("@passwordHash", PasswordHashing.Hash(txtNewPassword.Text));
                        cmd2.Parameters.AddWithValue("@username", txtUsername.Text);
                        cmd2.ExecuteNonQuery();
                    }

                    // Save the picture only if the user chose a file
                    if (UploadPic.HasFile)
                    {
                        string ImgPath = "~/ProfilePicture/" + Session["UserId"].ToString() + ext;

                        //saving the photo to the file directory
                        UploadPic.SaveAs(Server.MapPath(ImgPath));

                        string query3 = "UPDATE Users SET ProfilePicture = @picture WHERE Username = @username";
                        SqlCommand cmd3 = new SqlCommand(query3, con);
                        cmd3.Parameters.AddWithValue("@picture", ImgPath);
                        cmd3.Parameters.AddWithValue("@username", txtUsername.Text);
                        cmd3.ExecuteNonQuery();
                    }

                    con.Close();
                    Response.Redirect("EditProfile.aspx");
                }
            }
            catch (Exception ex)
            {
                lblMessage.Visible = true;
                lblMessage.Text = "An error occurred while saving your profile. " + ex.Message;
            }
        }
    }
}