
using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace WAPP_EduNexus_App
{
    public partial class AdminManageCourse : System.Web.UI.Page
    {
        // Page_Init removed — rely on the markup OnClick attribute to wire btnSaveCourse_Click.
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Initial page setup goes here.
                LoadCourses();
            }
        }

        protected void rptAdminCourses_ItemCommand(
            object source, RepeaterCommandEventArgs e)
        {
             //DELETE COURSE
            if (e.CommandName == "DeleteCourse")
            {
                int id;

                if (!int.TryParse(e.CommandArgument?.ToString(), out id))
                {
                    ShowModalMessage("Invalid course ID.", false);
                    return;
                }

                string connStr =
                    ConfigurationManager.ConnectionStrings[
                        "ConnectionString"].ConnectionString;

                try
                {
                    using (var con = new SqlConnection(connStr))
                    {
                        con.Open();

                        using (var transaction = con.BeginTransaction())
                        {
                            try
                            {
                                // Delete related materials first
                                using (var cmdMaterials = new SqlCommand(
                                    "DELETE FROM dbo.MaterialTable WHERE courseID = @id",
                                    con, transaction))
                                {
                                    cmdMaterials.Parameters.Add(
                                        "@id", SqlDbType.Int).Value = id;

                                    cmdMaterials.ExecuteNonQuery();
                                }

                                // Delete the course
                                using (var cmdCourse = new SqlCommand(
                                    "DELETE FROM dbo.CourseDetailTable WHERE courseID = @id",
                                    con, transaction))
                                {
                                    cmdCourse.Parameters.Add(
                                        "@id", SqlDbType.Int).Value = id;

                                    int rowsDeleted = cmdCourse.ExecuteNonQuery();

                                    if (rowsDeleted == 0)
                                    {
                                        throw new InvalidOperationException(
                                            "Course not found.");
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
                                    // Preserve the original error
                                }

                                throw;
                            }
                        }
                    }

                    // Refresh course list after successful deletion
                    LoadCourses();
                }
                catch (Exception ex)
                {
                    ShowModalMessage(
                        "Error deleting course: " + ex.Message,
                        false);
                }
            }
            // EDIT COURSE - THIS MUST BE OUTSIDE THE DELETE CONDITION
            else if (e.CommandName == "EditCourse")
            {
                int id;

                if (int.TryParse(e.CommandArgument?.ToString(), out id))
                {
                    Response.Redirect(
                        ResolveUrl(
                            "AdminManageCourseDetail.aspx?courseId=" + id));
                }
                else
                {
                    ShowModalMessage("Invalid course ID for editing.", false);
                }
            }
        }

        protected void btnCreateCourse_Click(object sender, EventArgs e)
        {
            Response.Redirect("AdminManageCourseDetail.aspx");
        }

        private void LoadCourseIntoForm(int courseId)
        {
            string connStr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            try
            {
                using (var con = new SqlConnection(connStr))
                using (var cmd = new SqlCommand("SELECT TOP 1 * FROM CourseDetailTable WHERE courseID = @id", con))
                {
                    cmd.Parameters.AddWithValue("@id", courseId);
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        if (dt.Rows.Count == 1)
                        {
                            var row = dt.Rows[0];

                            if (dt.Columns.Contains("courseTitle")) txtCourseName.Text = row["courseTitle"] as string ?? string.Empty;
                            if (dt.Columns.Contains("Instructor")) txtInstructor.Text = row["Instructor"] as string ?? string.Empty;
                            if (dt.Columns.Contains("Description")) txtDescription.Text = row["Description"] as string ?? string.Empty;
                            if (dt.Columns.Contains("level"))
                            {
                                var lvl = row["level"] as string ?? string.Empty;
                                if (ddlCourseLevel.Items.FindByValue(lvl) != null) ddlCourseLevel.SelectedValue = lvl;
                            }
                            if (dt.Columns.Contains("Price") && row["Price"] != DBNull.Value)
                            {
                                try
                                {
                                    txtPrice.Text = Convert.ToDecimal(row["Price"]).ToString(System.Globalization.CultureInfo.InvariantCulture);
                                }
                                catch
                                {
                                    txtPrice.Text = string.Empty;
                                }
                            }
                            if (dt.Columns.Contains("Image") && row["Image"] != DBNull.Value)
                            {
                                try
                                {
                                    var img = row["Image"] as string;
                                    if (!string.IsNullOrEmpty(img) && imgPreview != null)
                                    {
                                        imgPreview.ImageUrl = ResolveUrl("~/" + img.TrimStart('/'));
                                        imgPreview.Visible = true;
                                    }
                                }
                                catch
                                {
                                    // ignore preview errors
                                }
                            }

                            // set hidden field so save knows we're editing
                            if (hfEditingCourseId != null) hfEditingCourseId.Value = courseId.ToString();

                            // open modal on client (page should define openForm or similar)
                            ClientScript.RegisterStartupScript(this.GetType(), "openCourseModal", "openForm();", true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowModalMessage("Error loading course: " + ex.Message, true);
            }
        }
        protected void btnApply_Click(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim();
            string category = ddlCategory.SelectedValue;
            string level = ddlLevel.SelectedValue;

            // Apply filters and reload the course list
            LoadCourses(searchText, category, level);
        }

        protected void btnSaveCourse_Click(object sender, EventArgs e)
        {
            // Get values from the form
            string courseTitle = txtCourseName.Text.Trim();
            string instructor = txtInstructor.Text.Trim();
            string priceText = txtPrice.Text.Trim();
            //string category = ddlCourseCategory.SelectedValue;
            string level = ddlCourseLevel.SelectedValue;
            string description = txtDescription.Text.Trim();

            // Validate required fields
            if (string.IsNullOrWhiteSpace(courseTitle))
            {
                ShowModalMessage("Please complete all required fields.", true);
                return;
            }

            // TODO: Insert the course into your database here.
            //
            // Values ready for database insertion:
            // courseName
            // courseCode
            // category
            // level
            // description
            //
            // Do not display a "saved successfully" message
            // until the database insert has succeeded.

            // Temporary confirmation while database integration
            // is not yet implemented.
            // Insert into CourseDetailTable using available columns
            string connStr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            // handle image upload if present
            string imagePath = null;
            if (fuImage != null && fuImage.HasFile)
            {
                var allowed = new[] { ".png", ".jpg", ".jpeg", ".gif" };
                string ext = Path.GetExtension(fuImage.FileName).ToLowerInvariant();
                if (Array.IndexOf(allowed, ext) >= 0)
                {
                    string fileName = Guid.NewGuid().ToString("N") + ext;
                    string saveDir = Server.MapPath("~/IMG/");
                    Directory.CreateDirectory(saveDir);
                    string fullPath = Path.Combine(saveDir, fileName);
                    fuImage.SaveAs(fullPath);
                    imagePath = "IMG/" + fileName;
                }
            }
            try
            {
                using (var con = new SqlConnection(connStr))
                {
                    con.Open();

                    // Discover which columns exist in the table
                    DataTable schemaTable = new DataTable();
                    using (var cmd = new SqlCommand("SELECT TOP 1 * FROM CourseDetailTable", con))
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(schemaTable);
                    }

                    var availableCols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (DataColumn c in schemaTable.Columns) availableCols.Add(c.ColumnName);

                    var insertCols = new List<string>();
                    var insertParams = new List<string>();
                    var parameters = new List<SqlParameter>();

                    if (availableCols.Contains("courseTitle"))
                    {
                        insertCols.Add("courseTitle");
                        insertParams.Add("@courseTitle");
                        parameters.Add(new SqlParameter("@courseTitle", SqlDbType.NVarChar, 255) { Value = (object)courseTitle ?? DBNull.Value });
                    }
                    if (availableCols.Contains("Instructor"))
                    {
                        insertCols.Add("Instructor");
                        insertParams.Add("@Instructor");
                        parameters.Add(new SqlParameter("@Instructor", SqlDbType.NVarChar, 255) { Value = (object)instructor ?? DBNull.Value });
                    }
                    if (availableCols.Contains("Description"))
                    {
                        insertCols.Add("Description");
                        insertParams.Add("@Description");
                        parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar, -1) { Value = (object)description ?? DBNull.Value });
                    }
                    if (availableCols.Contains("Category"))
                    {
                        insertCols.Add("Category");
                        insertParams.Add("@Category");
                        parameters.Add(new SqlParameter("@Category", SqlDbType.NVarChar, 100) { Value = (object)ddlCategory.SelectedValue ?? DBNull.Value });
                    }
                    if (availableCols.Contains("level"))
                    {
                        insertCols.Add("level");
                        insertParams.Add("@level");
                        parameters.Add(new SqlParameter("@level", SqlDbType.NVarChar, 50) { Value = (object)level ?? DBNull.Value });
                    }
                    if (availableCols.Contains("Image"))
                    {
                        insertCols.Add("Image");
                        insertParams.Add("@Image");
                        parameters.Add(new SqlParameter("@Image", SqlDbType.NVarChar, 255) { Value = (object)imagePath ?? DBNull.Value });
                    }
                    if (availableCols.Contains("Price"))
                    {
                        insertCols.Add("Price");
                        insertParams.Add("@Price");
                        decimal priceVal;
                        if (decimal.TryParse(priceText, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out priceVal))
                        {
                            var p = new SqlParameter("@Price", SqlDbType.Decimal) { Value = priceVal };
                            p.Precision = 18;
                            p.Scale = 2;
                            parameters.Add(p);
                        }
                        else
                        {
                            parameters.Add(new SqlParameter("@Price", SqlDbType.Decimal) { Value = DBNull.Value });
                        }
                    }

                    if (insertCols.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(hfEditingCourseId?.Value))
                        {
                            // UPDATE existing course
                            var sets = new List<string>();
                            for (int i = 0; i < insertCols.Count; i++) sets.Add(insertCols[i] + " = " + insertParams[i]);
                            string sql = $"UPDATE CourseDetailTable SET {string.Join(",", sets)} WHERE courseID = @courseID";
                            using (var cmd = new SqlCommand(sql, con))
                            {
                                cmd.Parameters.AddRange(parameters.ToArray());
                                cmd.Parameters.AddWithValue("@courseID", hfEditingCourseId.Value);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            string sql = $"INSERT INTO CourseDetailTable ({string.Join(",", insertCols)}) VALUES ({string.Join(",", insertParams)})";
                            using (var cmd = new SqlCommand(sql, con))
                            {
                                cmd.Parameters.AddRange(parameters.ToArray());
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }

                // Redirect to the same page to clear the POST and refresh the list (prevents duplicate inserts on F5)
                Response.Redirect(Request.RawUrl);
            }
            catch (Exception ex)
            {
                ShowModalMessage("Error saving course: " + ex.Message, true);
            }
        }

        private void ShowModalMessage(string message, bool keepModalOpen)
        {
            string safeMessage = HttpUtility.JavaScriptStringEncode(message);

            string script = "alert('" + safeMessage + "');";

            if (keepModalOpen)
            {
                script += "document.getElementById('courseModal').style.display = 'flex';";
            }

            ClientScript.RegisterStartupScript(
                this.GetType(),
                "CourseFormMessage",
                script,
                true
            );
        }


        private void LoadCourses(
            string searchText = null,
            string category = null,
            string level = null)
        {
            string connStr =
                ConfigurationManager.ConnectionStrings[
                    "ConnectionString"].ConnectionString;

            var dt = new DataTable();

            try
            {
                using (var con = new SqlConnection(connStr))
                {
                    con.Open();

                    // Count all courses in the database
                    using (var countCmd = new SqlCommand(
                        "SELECT COUNT(*) FROM dbo.CourseDetailTable", con))
                    {
                        int totalCourses = Convert.ToInt32(
                            countCmd.ExecuteScalar());

                        ltTotalCourseCount.Text = totalCourses.ToString("N0");
                    }

                    // Read CourseDetailTable column information
                    var courseSchema = new DataTable();

                    using (var cmd = new SqlCommand(
                        "SELECT TOP 0 * FROM dbo.CourseDetailTable", con))
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(courseSchema);
                    }

                    var courseCols = new HashSet<string>(
                        StringComparer.OrdinalIgnoreCase);

                    foreach (DataColumn col in courseSchema.Columns)
                        courseCols.Add(col.ColumnName);

                    // Read CourseCategory column information
                    var categorySchema = new DataTable();

                    using (var cmd = new SqlCommand(
                        "SELECT TOP 0 * FROM dbo.CourseCategory", con))
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(categorySchema);
                    }

                    bool hasCategory = courseCols.Contains("Category");

                    bool hasCategoryLookup =
                        hasCategory && categorySchema.Columns.Count >= 2;

                    string categoryIdColumn = hasCategoryLookup
                        ? categorySchema.Columns[0].ColumnName
                        : "";

                    string categoryNameColumn = hasCategoryLookup
                        ? categorySchema.Columns[1].ColumnName
                        : "";

                    // Safely quote column names obtained from the database schema
                    Func<string, string> q = name =>
                        "[" + name.Replace("]", "]]") + "]";

                    bool hasLevel = courseCols.Contains("level");
                    bool hasDescription = courseCols.Contains("Description");
                    bool hasInstructor = courseCols.Contains("Instructor");
                    bool hasPrice = courseCols.Contains("Price");

                    // Find the available image/thumbnail column
                    string thumbnailExpr = "NULL AS Thumbnail";

                    if (courseCols.Contains("Image"))
                        thumbnailExpr = "cd.[Image] AS Thumbnail";
                    else if (courseCols.Contains("Thumbnail_img"))
                        thumbnailExpr = "cd.[Thumbnail_img] AS Thumbnail";
                    else if (courseCols.Contains("Thumbnail"))
                        thumbnailExpr = "cd.[Thumbnail] AS Thumbnail";

                    var sql = new System.Text.StringBuilder();

                    sql.Append("SELECT cd.courseID, cd.courseTitle");

                    // Fetch the category display name from CourseCategory
                    if (hasCategoryLookup)
                    {
                        sql.Append(
                            ", COALESCE(CONVERT(NVARCHAR(255), cc." +
                            q(categoryNameColumn) +
                            "), CONVERT(NVARCHAR(255), cd.[Category])) AS Category");
                    }
                    else if (hasCategory)
                    {
                        sql.Append(", cd.[Category] AS Category");
                    }
                    else
                    {
                        sql.Append(", CAST(NULL AS NVARCHAR(255)) AS Category");
                    }

                    sql.Append(hasLevel
                        ? ", cd.[level] AS [level]"
                        : ", CAST(NULL AS NVARCHAR(50)) AS [level]");

                    sql.Append(hasDescription
                        ? ", cd.[Description] AS MetaInfo"
                        : ", CAST(NULL AS NVARCHAR(MAX)) AS MetaInfo");

                    sql.Append(hasInstructor
                        ? ", cd.[Instructor] AS InstructorName"
                        : ", CAST(NULL AS NVARCHAR(255)) AS InstructorName");

                    sql.Append(hasPrice
                        ? ", cd.[Price] AS Price"
                        : ", CAST(NULL AS DECIMAL(18,2)) AS Price");

                    sql.Append(
                        ", " + thumbnailExpr +
                        ", NULL AS InstructorInitials, 0 AS Students");

                    sql.Append(" FROM dbo.CourseDetailTable AS cd");

                    // Match either the stored category ID or an existing category name.
                    // This also supports older courses that may store the name directly.
                    if (hasCategoryLookup)
                    {
                        string storedCategory =
                            "LTRIM(RTRIM(CONVERT(NVARCHAR(255), cd.[Category])))";

                        sql.Append(
                            " LEFT JOIN dbo.CourseCategory AS cc ON (" +
                            storedCategory + " = LTRIM(RTRIM(CONVERT(NVARCHAR(255), cc." +
                            q(categoryIdColumn) + "))) OR " +
                            storedCategory + " = LTRIM(RTRIM(CONVERT(NVARCHAR(255), cc." +
                            q(categoryNameColumn) + "))))");
                    }

                    sql.Append(" WHERE 1 = 1");

                    var cmdQuery = new SqlCommand();
                    cmdQuery.Connection = con;

                    // Search only columns that exist
                    var searchConditions = new List<string>();

                    if (courseCols.Contains("courseTitle"))
                        searchConditions.Add("cd.[courseTitle] LIKE @search");

                    if (hasDescription)
                        searchConditions.Add("cd.[Description] LIKE @search");

                    if (hasInstructor)
                        searchConditions.Add("cd.[Instructor] LIKE @search");

                    if (!string.IsNullOrWhiteSpace(searchText) &&
                        searchConditions.Count > 0)
                    {
                        sql.Append(
                            " AND (" + string.Join(" OR ", searchConditions) + ")");

                        cmdQuery.Parameters.AddWithValue(
                            "@search", "%" + searchText.Trim() + "%");
                    }

                    // Category filter uses the category name from CourseCategory
                    if (!string.IsNullOrWhiteSpace(category))
                    {
                        if (hasCategoryLookup)
                        {
                            sql.Append(
                                " AND (cc." + q(categoryNameColumn) +
                                " = @category OR cd.[Category] = @category)");
                        }
                        else if (hasCategory)
                        {
                            sql.Append(" AND cd.[Category] = @category");
                        }

                        cmdQuery.Parameters.AddWithValue("@category", category);
                    }

                    if (!string.IsNullOrWhiteSpace(level) && hasLevel)
                    {
                        sql.Append(" AND cd.[level] = @level");
                        cmdQuery.Parameters.AddWithValue("@level", level);
                    }

                    sql.Append(" ORDER BY cd.courseID DESC");

                    cmdQuery.CommandText = sql.ToString();

                    using (var da = new SqlDataAdapter(cmdQuery))
                    {
                        da.Fill(dt);
                    }

                    cmdQuery.Dispose();
                }

                // Display the courses
                if (dt.Rows.Count == 0)
                {
                    ltCoursesMessage.Text =
                        "<div class='no-courses'>No courses found.</div>";

                    rptAdminCourses.Visible = false;
                    ltCoursesCount.Text = "";
                }
                else
                {
                    ltCoursesMessage.Text = "";
                    rptAdminCourses.Visible = true;
                    rptAdminCourses.DataSource = dt;
                    rptAdminCourses.DataBind();

                    ltCoursesCount.Text =
                        $"<div class='courses-count'>Showing {dt.Rows.Count} courses</div>";
                }
            }
            catch (Exception ex)
            {
                ltCoursesMessage.Text =
                    "<div class='error'>Error loading courses: " +
                    HttpUtility.HtmlEncode(ex.Message) + "</div>";
            }
        }

        // Client-side modal functions are implemented in the ASPX markup as JavaScript.
    }
}