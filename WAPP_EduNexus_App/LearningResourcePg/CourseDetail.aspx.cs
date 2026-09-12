using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP_EduNexus_App
{
    public partial class CourseDetail : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string courseId = Request.QueryString["courseId"];

                if (!string.IsNullOrEmpty(courseId))
                {
                    LoadCourse(courseId);
                }
            }
        }

        protected void LoadCourse(string courseId)
        {
            string connectionString =
                ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT courseID, courseTitle, level, Category
                    FROM CourseDetailTable
                    WHERE courseID = @courseID";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@courseID", courseId);

                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lblBreadcrumbCourse.Text = reader["Category"].ToString();
                            lblCourseTitle.Text = reader["courseTitle"].ToString();
                            lblCategory.Text = reader["Category"].ToString();
                            lblLevel.Text = reader["level"].ToString();
                        }
                    }
                }
            }
        }

        protected void SqlDataSource1_Selecting(object sender, SqlDataSourceSelectingEventArgs e)
        {

        }
    }
}