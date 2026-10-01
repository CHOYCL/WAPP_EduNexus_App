
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
                // Initial page setup can go here.
                LoadCourses();
            }
        }

        protected void rptAdminCourses_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "DeleteCourse")
            {
                int id;
                if (int.TryParse(e.CommandArgument?.ToString(), out id))
                {
                    try
                    {
                        string connStr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                        using (var con = new SqlConnection(connStr))
                        using (var cmd = new SqlCommand("DELETE FROM CourseDetailTable WHERE courseID = @id", con))
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            con.Open();
                            cmd.ExecuteNonQuery();
                        }
                        LoadCourses();
                    }
                    catch (Exception ex)
                    {
                        ShowModalMessage("Error deleting course: " + ex.Message, false);
                    }
                }
            }
            else if (e.CommandName == "EditCourse")
            {
                int id;
                if (int.TryParse(e.CommandArgument?.ToString(), out id))
                {
                    LoadCourseIntoForm(id);
                }
            }
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

            // TODO:
            // Apply these filters to your course database query.
            // Example:
            // LoadCourses(searchText, category, level);
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

        private void LoadCourses()
        {
            string connStr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            var dt = new DataTable();
            try
            {
                using (var con = new SqlConnection(connStr))
                {
                    con.Open();

                    // Detect whether Image column exists and build SELECT accordingly to avoid SQL errors
                    var schemaCols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    using (var schemaCmd = new SqlCommand("SELECT TOP 1 * FROM CourseDetailTable", con))
                    using (var schemaAdapter = new SqlDataAdapter(schemaCmd))
                    {
                        var schemaTable = new DataTable();
                        schemaAdapter.Fill(schemaTable);
                        foreach (DataColumn c in schemaTable.Columns) schemaCols.Add(c.ColumnName);
                    }

                    bool hasImage = schemaCols.Contains("Image");
                    string thumbnailExpr = hasImage ? "Image AS Thumbnail" : "NULL AS Thumbnail";

                    string sql = $"SELECT courseID, courseTitle, Category, level, Description AS MetaInfo, Instructor AS InstructorName, Price, {thumbnailExpr}, NULL AS InstructorInitials, 0 AS Students FROM CourseDetailTable ORDER BY courseID DESC";

                    using (var cmd = new SqlCommand(sql, con))
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                if (dt.Rows.Count == 0)
                {
                    ltCoursesMessage.Text = "<div class='no-courses'>No courses found.</div>";
                    rptAdminCourses.Visible = false;
                    ltCoursesCount.Text = string.Empty;
                }
                else
                {
                    ltCoursesMessage.Text = string.Empty;
                    rptAdminCourses.Visible = true;
                    rptAdminCourses.DataSource = dt;
                    rptAdminCourses.DataBind();
                    ltCoursesCount.Text = $"<div class='courses-count'>Showing {dt.Rows.Count} courses</div>";
                }
            }
            catch (Exception ex)
            {
                // Show an admin-visible message so load errors are not silently ignored
                ltCoursesMessage.Text = "<div class='error'>Error loading courses: " + HttpUtility.HtmlEncode(ex.Message) + "</div>";
            }
        }
        // Client-side modal functions are implemented in the ASPX markup as JavaScript.
    }
}