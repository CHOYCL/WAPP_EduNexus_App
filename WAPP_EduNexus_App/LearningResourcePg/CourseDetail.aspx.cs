using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.UI;

namespace WAPP_EduNexus_App
{
    public partial class CourseDetail : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string courseId = Request.QueryString["courseId"];

                if (!string.IsNullOrWhiteSpace(courseId))
                {
                    LoadCourse(courseId);
                }
                else
                {
                    ltCourseTitle.Text = "Course not found.";
                }
            }
        }

        private string GetColumnValue(DataRow row, string columnName)
        {
            if (row != null &&
                row.Table.Columns.Contains(columnName) &&
                row[columnName] != DBNull.Value)
            {
                return Convert.ToString(row[columnName]);
            }

            return string.Empty;
        }

        protected void LoadCourse(string courseId)
        {
            int id;
            if (!int.TryParse(courseId, out id))
            {
                ltCourseTitle.Text = "Invalid course ID.";
                return;
            }

            string connectionString =
                ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    DataTable courseTable = new DataTable();

                    using (SqlCommand cmd = new SqlCommand(
                        @"SELECT TOP 1 *
                          FROM dbo.CourseDetailTable
                          WHERE courseID = @courseID", con))
                    {
                        cmd.Parameters.Add("@courseID", SqlDbType.Int).Value = id;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(courseTable);
                        }
                    }

                    if (courseTable.Rows.Count == 0)
                    {
                        ltCourseTitle.Text = "Course not found.";
                        return;
                    }

                    DataRow course = courseTable.Rows[0];
                    string title = GetColumnValue(course, "courseTitle");
                    string category = GetColumnValue(course, "Category");
                    string level = GetColumnValue(course, "level");
                    string instructor = GetColumnValue(course, "Instructor");
                    string description = GetColumnValue(course, "Description");

                    category = GetCategoryName(con, category);

                    // Display the course title safely.
                    ltCourseTitle.Text = HttpUtility.HtmlEncode(title);

                    // Display category.
                    if (string.IsNullOrWhiteSpace(category))
                    {
                        ltCategory.Text = string.Empty;
                    }
                    else
                    {
                        ltCategory.Text =
                            "<span class=\"tag-pill\">" +
                            HttpUtility.HtmlEncode(category) +
                            "</span>";
                    }

                    // Display level.
                    if (string.IsNullOrWhiteSpace(level))
                    {
                        ltLevel.Text = string.Empty;
                    }
                    else
                    {
                        ltLevel.Text =
                            "<span class=\"tag-pill\">" +
                            HttpUtility.HtmlEncode(level) +
                            "</span>";
                    }

                    // Display instructor.
                    if (string.IsNullOrWhiteSpace(instructor))
                    {
                        ltInstructor.Text = string.Empty;
                    }
                    else
                    {
                        ltInstructor.Text =
                            "<span class=\"tag-pill\">" +
                            HttpUtility.HtmlEncode(instructor) +
                            "</span>";
                    }

                    DataTable materialTable = new DataTable();
                    using (SqlCommand cmd = new SqlCommand(
                        @"SELECT TOP 1 *
                          FROM dbo.MaterialTable
                          WHERE courseID = @courseID
                          ORDER BY materialID DESC", con))
                    {
                        cmd.Parameters.Add("@courseID", SqlDbType.Int).Value = id;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(materialTable);
                        }
                    }

                    string mediaPath = string.Empty;
                    if (materialTable.Rows.Count > 0)
                    {
                        DataRow material = materialTable.Rows[0];
                        string materialDescription = GetColumnValue(material, "materialDescription");
                        if (!string.IsNullOrWhiteSpace(materialDescription))
                        {
                            description = materialDescription;
                        }

                        mediaPath = GetColumnValue(material, "materialType");
                        if (string.IsNullOrWhiteSpace(mediaPath)) mediaPath = GetColumnValue(material, "MediaPath");
                        if (string.IsNullOrWhiteSpace(mediaPath)) mediaPath = GetColumnValue(material, "Image");
                        if (string.IsNullOrWhiteSpace(mediaPath)) mediaPath = GetColumnValue(material, "VideoPath");
                        if (string.IsNullOrWhiteSpace(mediaPath)) mediaPath = GetColumnValue(material, "Thumbnail_img");
                        if (string.IsNullOrWhiteSpace(mediaPath)) mediaPath = GetColumnValue(material, "FilePath");
                    }

                    if (string.IsNullOrWhiteSpace(mediaPath)) mediaPath = GetColumnValue(course, "Thumbnail_img");
                    if (string.IsNullOrWhiteSpace(mediaPath)) mediaPath = GetColumnValue(course, "Image");
                    if (string.IsNullOrWhiteSpace(mediaPath)) mediaPath = GetColumnValue(course, "Thumbnail");

                    ltMaterialDescription.Text = HttpUtility.HtmlEncode(description);
                    DisplayCourseMedia(mediaPath);

                    // Show "Enrolled" if the signed-in user has already enrolled.
                    UpdateEnrollmentButtonState(con, id);
                }
            }
            catch (Exception)
            {
                ltCourseTitle.Text = "Unable to load this course. Please try again later.";
            }
        }

        private string GetCategoryName(SqlConnection con, string categoryValue)
        {
            if (string.IsNullOrWhiteSpace(categoryValue))
            {
                return string.Empty;
            }

            using (SqlCommand cmd = new SqlCommand(
                "SELECT TOP 100 * FROM dbo.CourseCategory", con))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataTable categories = new DataTable();
                da.Fill(categories);

                // Assumes column 0 is category ID and column 1 is category name.
                if (categories.Columns.Count >= 2)
                {
                    foreach (DataRow row in categories.Rows)
                    {
                        string categoryId = Convert.ToString(row[0]);
                        string categoryName = Convert.ToString(row[1]);

                        if (string.Equals(categoryId, categoryValue, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(categoryName, categoryValue, StringComparison.OrdinalIgnoreCase))
                        {
                            return categoryName;
                        }
                    }
                }
            }

            // Supports older records that store the category name directly.
            return categoryValue;
        }

        private void DisplayCourseMedia(string mediaPath)
        {
            imgCourseMedia.Visible = false;
            imgCourseMedia.ImageUrl = string.Empty;
            imgCourseMedia.Style["display"] = "none";

            courseVID.Style["display"] = "none";
            videoSource.Attributes["src"] = string.Empty;

            if (string.IsNullOrWhiteSpace(mediaPath))
            {
                return;
            }

            string path = mediaPath.Trim();
            string url;

            if (path.StartsWith("~/"))
            {
                url = ResolveUrl(path);
            }
            else if (path.StartsWith("/"))
            {
                url = ResolveUrl("~" + path);
            }
            else
            {
                url = ResolveUrl("~/" + path);
            }

            string extension = Path.GetExtension(path).ToLowerInvariant();
            if (extension == ".mp4")
            {
                videoSource.Attributes["src"] = url;
                courseVID.Style["display"] = "block";
            }
            else if (extension == ".jpg" || extension == ".jpeg" ||
                     extension == ".png" || extension == ".gif" ||
                     extension == ".bmp" || extension == ".webp")
            {
                imgCourseMedia.ImageUrl = url;
                imgCourseMedia.Visible = true;
                imgCourseMedia.Style["display"] = "block";
            }
        }

        protected void btnEnrollCourse_Click(object sender, EventArgs e)
        {
            lblEnrollMessage.Visible = false;
            lblEnrollMessage.Text = string.Empty;

            int userId;
            if (!int.TryParse(Convert.ToString(Session["UserId"]), out userId))
            {
                Response.Redirect(ResolveUrl("~/CommonPage/LoginPage.aspx"), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            int courseId;
            if (!int.TryParse(Request.QueryString["courseId"], out courseId))
            {
                ShowEnrollMessage("Invalid course ID.");
                return;
            }

            try
            {
                string connectionString =
                    ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Confirm the user still exists in the Users table.
                    using (SqlCommand userCmd = new SqlCommand(
                        "SELECT COUNT(*) FROM dbo.Users WHERE Id = @UserId", con))
                    {
                        userCmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                        if (Convert.ToInt32(userCmd.ExecuteScalar()) == 0)
                        {
                            Session.Remove("UserId");
                            Response.Redirect(ResolveUrl("~/CommonPage/LoginPage.aspx"), false);
                            Context.ApplicationInstance.CompleteRequest();
                            return;
                        }
                    }

                    using (SqlTransaction transaction = con.BeginTransaction(IsolationLevel.Serializable))
                    {
                        try
                        {
                            using (SqlCommand courseCmd = new SqlCommand(
                                "SELECT COUNT(*) FROM dbo.CourseDetailTable WHERE courseID = @CourseID",
                                con, transaction))
                            {
                                courseCmd.Parameters.Add("@CourseID", SqlDbType.Int).Value = courseId;
                                if (Convert.ToInt32(courseCmd.ExecuteScalar()) == 0)
                                {
                                    transaction.Rollback();
                                    ShowEnrollMessage("Course not found.");
                                    return;
                                }
                            }

                            using (SqlCommand checkCmd = new SqlCommand(
                                @"SELECT COUNT(*)
                                  FROM dbo.Enrollments WITH (UPDLOCK, HOLDLOCK)
                                  WHERE UserID = @UserID AND courseID = @CourseID",
                                con, transaction))
                            {
                                checkCmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                                checkCmd.Parameters.Add("@CourseID", SqlDbType.Int).Value = courseId;

                                if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
                                {
                                    transaction.Commit();
                                    SetEnrolledButtonState();
                                    ShowEnrollMessage("You are already enrolled in this course.");
                                    return;
                                }
                            }

                            bool enrollmentIdIsIdentity;
                            using (SqlCommand identityCmd = new SqlCommand(
                                @"SELECT COLUMNPROPERTY(
                                      OBJECT_ID('dbo.Enrollments'),
                                      'EnrollmentID', 'IsIdentity')",
                                con, transaction))
                            {
                                object result = identityCmd.ExecuteScalar();
                                enrollmentIdIsIdentity = result != null &&
                                    result != DBNull.Value && Convert.ToInt32(result) == 1;
                            }

                            if (enrollmentIdIsIdentity)
                            {
                                using (SqlCommand insertCmd = new SqlCommand(
                                    @"INSERT INTO dbo.Enrollments
                                      (UserID, courseID, EnrollmentDate, [Status])
                                      VALUES (@UserID, @CourseID, @EnrollmentDate, @Status)",
                                    con, transaction))
                                {
                                    AddEnrollmentParameters(insertCmd, userId, courseId);
                                    insertCmd.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                int enrollmentId;
                                using (SqlCommand idCmd = new SqlCommand(
                                    @"SELECT ISNULL(MAX(EnrollmentID), 0) + 1
                                      FROM dbo.Enrollments WITH (UPDLOCK, HOLDLOCK)",
                                    con, transaction))
                                {
                                    enrollmentId = Convert.ToInt32(idCmd.ExecuteScalar());
                                }

                                using (SqlCommand insertCmd = new SqlCommand(
                                    @"INSERT INTO dbo.Enrollments
                                      (EnrollmentID, UserID, courseID, EnrollmentDate, [Status])
                                      VALUES (@EnrollmentID, @UserID, @CourseID, @EnrollmentDate, @Status)",
                                    con, transaction))
                                {
                                    insertCmd.Parameters.Add("@EnrollmentID", SqlDbType.Int).Value = enrollmentId;
                                    AddEnrollmentParameters(insertCmd, userId, courseId);
                                    insertCmd.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                        }
                        catch
                        {
                            try
                            {
                                transaction.Rollback();
                            }
                            catch
                            {
                                // Preserve the original error.
                            }
                            throw;
                        }
                    }
                }

                // Update the button immediately after a successful enrollment.
                SetEnrolledButtonState();
                ShowEnrollMessage("Successfully enrolled! This course is now in your learning list.");
            }
            catch (Exception)
            {
                ShowEnrollMessage("Unable to enroll in this course. Please try again.");
            }
        }

        private void UpdateEnrollmentButtonState(SqlConnection con, int courseId)
        {
            // Default state for guests or users who are not enrolled.
            btnEnrollCourse.Text = "Enroll Course";
            btnEnrollCourse.Enabled = true;

            int userId;
            if (!int.TryParse(Convert.ToString(Session["UserId"]), out userId))
            {
                return;
            }

            using (SqlCommand cmd = new SqlCommand(
                @"SELECT COUNT(*)
                  FROM dbo.Enrollments
                  WHERE UserID = @UserID AND courseID = @CourseID", con))
            {
                cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@CourseID", SqlDbType.Int).Value = courseId;

                bool isEnrolled = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                if (isEnrolled)
                {
                    SetEnrolledButtonState();
                }
            }
        }

        private void SetEnrolledButtonState()
        {
            btnEnrollCourse.Text = "Enrolled";
            btnEnrollCourse.Enabled = false;
        }

        private void AddEnrollmentParameters(SqlCommand cmd, int userId, int courseId)
        {
            cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
            cmd.Parameters.Add("@CourseID", SqlDbType.Int).Value = courseId;
            cmd.Parameters.Add("@EnrollmentDate", SqlDbType.Date).Value = DateTime.Today;
            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = "In Progress";
        }

        private void ShowEnrollMessage(string message)
        {
            lblEnrollMessage.Text = HttpUtility.HtmlEncode(message);
            lblEnrollMessage.Visible = true;
        }
    }
}
