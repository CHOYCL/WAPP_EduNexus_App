
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
                ConfigurationManager.ConnectionStrings[
                    "ConnectionString"].ConnectionString;

            try
            {
                using (SqlConnection con =
                    new SqlConnection(connectionString))
                {
                    con.Open();

                    // Load course information
                    DataTable courseTable = new DataTable();

                    using (SqlCommand cmd = new SqlCommand(
                        @"SELECT TOP 1 *
                          FROM dbo.CourseDetailTable
                          WHERE courseID = @courseID", con))
                    {
                        cmd.Parameters.Add(
                            "@courseID", SqlDbType.Int).Value = id;

                        using (SqlDataAdapter da =
                            new SqlDataAdapter(cmd))
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

                    string title =
                        GetColumnValue(course, "courseTitle");

                    string category =
                        GetColumnValue(course, "Category");

                    string level =
                        GetColumnValue(course, "level");

                    string instructor =
                        GetColumnValue(course, "Instructor");

                    string description =
                        GetColumnValue(course, "Description");

                    // Resolve the stored category ID to its display name
                    category = GetCategoryName(con, category);

                    // Display course information safely
                    ltCourseTitle.Text =
                        HttpUtility.HtmlEncode(title);

                    ltCategory.Text = string.IsNullOrWhiteSpace(category)
                        ? string.Empty
                        : "<span class=\"tag-pill\">" +
                          HttpUtility.HtmlEncode(category) +
                          "</span>";

                    ltLevel.Text = string.IsNullOrWhiteSpace(level)
                        ? string.Empty
                        : "<span class=\"tag-pill\">" +
                          HttpUtility.HtmlEncode(level) +
                          "</span>";

                    ltInstructor.Text = string.IsNullOrWhiteSpace(instructor)
                        ? string.Empty
                        : "<span class=\"tag-pill\">" +
                          HttpUtility.HtmlEncode(instructor) +
                          "</span>";

                    // Load the latest course material
                    DataTable materialTable = new DataTable();

                    using (SqlCommand cmd = new SqlCommand(
                        @"SELECT TOP 1 *
                          FROM dbo.MaterialTable
                          WHERE courseID = @courseID
                          ORDER BY materialID DESC", con))
                    {
                        cmd.Parameters.Add(
                            "@courseID", SqlDbType.Int).Value = id;

                        using (SqlDataAdapter da =
                            new SqlDataAdapter(cmd))
                        {
                            da.Fill(materialTable);
                        }
                    }

                    string mediaPath = string.Empty;

                    if (materialTable.Rows.Count > 0)
                    {
                        DataRow material = materialTable.Rows[0];

                        string materialDescription =
                            GetColumnValue(
                                material, "materialDescription");

                        if (!string.IsNullOrWhiteSpace(
                            materialDescription))
                        {
                            description = materialDescription;
                        }

                        // Find the stored media path
                        mediaPath =
                            GetColumnValue(material, "materialType");

                        if (string.IsNullOrWhiteSpace(mediaPath))
                            mediaPath = GetColumnValue(material, "MediaPath");

                        if (string.IsNullOrWhiteSpace(mediaPath))
                            mediaPath = GetColumnValue(material, "Image");

                        if (string.IsNullOrWhiteSpace(mediaPath))
                            mediaPath = GetColumnValue(material, "VideoPath");

                        if (string.IsNullOrWhiteSpace(mediaPath))
                            mediaPath = GetColumnValue(material, "Thumbnail_img");

                        if (string.IsNullOrWhiteSpace(mediaPath))
                            mediaPath = GetColumnValue(material, "FilePath");
                    }

                    // Use course-level image fields if material has no media path
                    if (string.IsNullOrWhiteSpace(mediaPath))
                        mediaPath = GetColumnValue(course, "Thumbnail_img");

                    if (string.IsNullOrWhiteSpace(mediaPath))
                        mediaPath = GetColumnValue(course, "Image");

                    if (string.IsNullOrWhiteSpace(mediaPath))
                        mediaPath = GetColumnValue(course, "Thumbnail");

                    ltMaterialDescription.Text =
                        HttpUtility.HtmlEncode(description);

                    // Display the course image or video
                    DisplayCourseMedia(mediaPath);
                }
            }
            catch (Exception)
            {
                ltCourseTitle.Text =
                    "Unable to load this course. Please try again later.";
            }
        }

        private string GetCategoryName(
            SqlConnection con, string categoryValue)
        {
            if (string.IsNullOrWhiteSpace(categoryValue))
                return string.Empty;

            using (SqlCommand cmd = new SqlCommand(
                "SELECT TOP 100 * FROM dbo.CourseCategory", con))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataTable categories = new DataTable();
                da.Fill(categories);

                // Assumes column 0 is category ID and column 1 is its name
                if (categories.Columns.Count >= 2)
                {
                    foreach (DataRow row in categories.Rows)
                    {
                        string categoryId =
                            Convert.ToString(row[0]);

                        string categoryName =
                            Convert.ToString(row[1]);

                        if (string.Equals(
                                categoryId, categoryValue,
                                StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(
                                categoryName, categoryValue,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            return categoryName;
                        }
                    }
                }
            }

            // Supports older records that store the category name directly
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
                return;

            string path = mediaPath.Trim();
            string url;

            if (path.StartsWith("~/"))
                url = ResolveUrl(path);
            else if (path.StartsWith("/"))
                url = ResolveUrl("~" + path);
            else
                url = ResolveUrl("~/" + path);

            string extension =
                Path.GetExtension(path).ToLowerInvariant();

            if (extension == ".mp4")
            {
                videoSource.Attributes["src"] = url;
                courseVID.Style["display"] = "block";
            }
            else if (extension == ".jpg" ||
                     extension == ".jpeg" ||
                     extension == ".png" ||
                     extension == ".gif" ||
                     extension == ".bmp" ||
                     extension == ".webp")
            {
                imgCourseMedia.ImageUrl = url;
                imgCourseMedia.Visible = true;
                imgCourseMedia.Style["display"] = "block";
            }
        }
    }
}
