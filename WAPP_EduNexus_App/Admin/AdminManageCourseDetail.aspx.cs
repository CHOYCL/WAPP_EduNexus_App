using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace WAPP_EduNexus_App
{
    public partial class AdminManageCourseDetail : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadCategories();
                string courseId = Request.QueryString["courseId"];
                if (!string.IsNullOrEmpty(courseId))
                {
                    LoadCourseAndMaterial(courseId);
                }
            }
        }

        private void LoadCategories()
        {
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var con = new SqlConnection(connStr))
                using (var cmd = new SqlCommand("SELECT TOP 100 * FROM CourseCategory", con))
                using (var da = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    ddlCategory.Items.Clear();
                    ddlCategory.Items.Add(new ListItem("-- Select --", string.Empty));
                    foreach (DataRow r in dt.Rows)
                    {
                        string val = string.Empty;
                        string text = string.Empty;
                        if (dt.Columns.Count >= 2)
                        {
                            val = Convert.ToString(r[0] ?? string.Empty);
                            text = Convert.ToString(r[1] ?? val);
                        }
                        else if (dt.Columns.Count == 1)
                        {
                            val = Convert.ToString(r[0] ?? string.Empty);
                            text = val;
                        }
                        if (!string.IsNullOrEmpty(text)) ddlCategory.Items.Add(new ListItem(HttpUtility.HtmlEncode(text), val));
                    }
                }
            }
            catch
            {
                // swallow: categories are non-critical for page load
            }
        }

        private void LoadCourseAndMaterial(string courseId)
        {
            string connStr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            try
            {
                using (var con = new SqlConnection(connStr))
                {
                    con.Open();

                    // Load course row (schema-aware) and populate display + edit controls
                    using (var cmd = new SqlCommand("SELECT TOP 1 * FROM CourseDetailTable WHERE courseID = @id", con))
                    {
                        cmd.Parameters.AddWithValue("@id", courseId);
                        using (var da = new SqlDataAdapter(cmd))
                        {
                            var dtCourse = new DataTable();
                            da.Fill(dtCourse);
                            if (dtCourse.Rows.Count > 0)
                            {
                                var row = dtCourse.Rows[0];
                                string title = string.Empty;
                                string cat = string.Empty;
                                string lvl = string.Empty;
                                string instr = string.Empty;
                                string price = string.Empty;

                                if (dtCourse.Columns.Contains("courseTitle") && row["courseTitle"] != DBNull.Value)
                                    title = Convert.ToString(row["courseTitle"]);
                                if (dtCourse.Columns.Contains("Category") && row["Category"] != DBNull.Value)
                                    cat = Convert.ToString(row["Category"]);
                                if (dtCourse.Columns.Contains("level") && row["level"] != DBNull.Value)
                                    lvl = Convert.ToString(row["level"]);
                                if (dtCourse.Columns.Contains("Instructor") && row["Instructor"] != DBNull.Value)
                                    instr = Convert.ToString(row["Instructor"]);
                                if (dtCourse.Columns.Contains("Price") && row["Price"] != DBNull.Value)
                                    price = Convert.ToString(row["Price"]);

                                ltCourseTitle.Text = HttpUtility.HtmlEncode(title);
                                // Get the category name corresponding to the saved category value
                                string categoryDisplay = cat;

                                ListItem categoryItem = ddlCategory.Items.FindByValue(cat);

                                if (categoryItem != null)
                                {
                                    // Display the category name, not its database ID
                                    categoryDisplay = categoryItem.Text;
                                }
                                else if (!string.IsNullOrEmpty(cat))
                                {
                                    // Fallback for older records that store category names directly
                                    categoryDisplay = cat;
                                }

                                ltCategory.Text = string.IsNullOrEmpty(categoryDisplay)
                                    ? string.Empty
                                    : $"<span class=\"tag-pill\">{HttpUtility.HtmlEncode(categoryDisplay)}</span>";

                                ltLevel.Text = string.IsNullOrEmpty(lvl) ? string.Empty : $"<span class=\"tag-pill\">{HttpUtility.HtmlEncode(lvl)}</span>";
                                ltInstructor.Text = string.IsNullOrEmpty(instr) ? string.Empty : $"<span class=\"tag-pill\">{HttpUtility.HtmlEncode(instr)}</span>";

                                // Populate edit controls so when admin clicks Edit they see saved values
                                try
                                {
                                    txtTitleEdit.Text = title;
                                }
                                catch { }
                                try
                                {
                                    if (!string.IsNullOrEmpty(cat))
                                    {
                                        var it = ddlCategory.Items.FindByValue(cat);
                                        if (it == null) ddlCategory.Items.Add(new ListItem(HttpUtility.HtmlEncode(cat), cat));
                                        ddlCategory.SelectedValue = cat;
                                    }
                                }
                                catch { }
                                try
                                {
                                    if (!string.IsNullOrEmpty(lvl))
                                    {
                                        var it2 = ddlLevel.Items.FindByValue(lvl);
                                        if (it2 == null) ddlLevel.Items.Add(new ListItem(HttpUtility.HtmlEncode(lvl), lvl));
                                        ddlLevel.SelectedValue = lvl;
                                    }
                                }
                                catch { }
                                try { txtInstructorEdit.Text = instr; } catch { }
                                try { txtPriceEdit.Text = price; } catch { }
                            }
                        }
                    }

                    // Load latest material for this course
                    string matQuery = "SELECT TOP 1 * FROM MaterialTable WHERE courseID = @id ORDER BY materialID DESC";
                    using (var cmd = new SqlCommand(matQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@id", courseId);
                        using (var da = new SqlDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            da.Fill(dt);
                            if (dt.Rows.Count > 0)
                            {
                                var row = dt.Rows[0];
                                string desc = string.Empty;
                                if (dt.Columns.Contains("materialDescription") && row["materialDescription"] != DBNull.Value)
                                    desc = Convert.ToString(row["materialDescription"]);

                                ltMaterialDescription.Text = HttpUtility.HtmlEncode(desc);

                                string mediaPath = null;
                                if (dt.Columns.Contains("materialType") && row["materialType"] != DBNull.Value)
                                    mediaPath = Convert.ToString(row["materialType"]);
                                else if (dt.Columns.Contains("MediaPath") && row["MediaPath"] != DBNull.Value)
                                    mediaPath = Convert.ToString(row["MediaPath"]);
                                else if (dt.Columns.Contains("Image") && row["Image"] != DBNull.Value)
                                    mediaPath = Convert.ToString(row["Image"]);
                                else if (dt.Columns.Contains("VideoPath") && row["VideoPath"] != DBNull.Value)
                                    mediaPath = Convert.ToString(row["VideoPath"]);

                    if (!string.IsNullOrEmpty(mediaPath))
                                {
                                    string url = ResolveUrl("~" + (mediaPath.StartsWith("/") ? mediaPath : "/" + mediaPath));
                                    var ext = Path.GetExtension(mediaPath).ToLowerInvariant();
                                    if (ext == ".mp4")
                                    {
                                        // show video (MP4 only)
                                        videoSource.Attributes["src"] = url;
                                        courseVID.Style["display"] = "block";
                                        imgCourseMedia.Visible = false;
                                    }
                                    else if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif" || ext == ".bmp" || ext == ".webp")
                                    {
                                        imgCourseMedia.ImageUrl = url;
                                        imgCourseMedia.Visible = true;
                                        courseVID.Style["display"] = "none";
                            imgCourseMedia.Style["display"] = "block";
                                    }
                                }
                            }
                            else
                            {
                                ltMaterialDescription.Text = string.Empty;
                            }
                        }
                    }
                }
            }
            catch (SqlException sx)
            {
                ltMaterialDescription.Text = "Error loading material: " + HttpUtility.HtmlEncode(sx.Message);
            }
            catch (Exception ex)
            {
                ltMaterialDescription.Text = "Error: " + HttpUtility.HtmlEncode(ex.Message);
            }
        }

        protected void btnUploadMedia_Click(object sender, EventArgs e)
        {
            string courseId = Request.QueryString["courseId"];
            if (string.IsNullOrEmpty(courseId))
            {
                // If no courseId in query but admin entered a new title via the client prompt
                // (stored in hfNewCourseTitle), create the course record first so media can be attached.
                var pendingTitle = (hfNewCourseTitle?.Value ?? string.Empty).Trim();
                if (!string.IsNullOrEmpty(pendingTitle))
                {
                    try
                    {
                        string connStr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                        using (var con = new SqlConnection(connStr))
                        {
                            con.Open();
                            using (var cmd = new SqlCommand("INSERT INTO CourseDetailTable (courseTitle, [level], Category, Instructor) VALUES (@title, @level, @category, @instructor); SELECT SCOPE_IDENTITY();", con))
                            {
                                cmd.Parameters.AddWithValue("@title", pendingTitle);
                                cmd.Parameters.AddWithValue("@level", "");
                                cmd.Parameters.AddWithValue("@category", "");
                                cmd.Parameters.AddWithValue("@instructor", "");
                                var obj = cmd.ExecuteScalar();
                                if (obj != null && int.TryParse(obj.ToString(), out var newId))
                                {
                                    courseId = newId.ToString();
                                    // update the querystring to reflect new course id on next refresh
                                    Response.Redirect("AdminManageCourseDetail.aspx?courseId=" + courseId);
                                    return;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ltMaterialDescription.Text = "Error creating course before upload: " + HttpUtility.HtmlEncode(ex.Message);
                        return;
                    }
                }

                ltMaterialDescription.Text = "Missing course id. Please save the course title first (click Edit -> Save).";
                return;
            }

            if (!fuMedia.HasFile)
            {
                ltMaterialDescription.Text = "Please choose a file to upload.";
                return;
            }

            var posted = fuMedia.PostedFile;
            var contentType = posted.ContentType.ToLowerInvariant();
            var ext = Path.GetExtension(fuMedia.FileName).ToLowerInvariant();

            bool isImage = contentType.StartsWith("image/") || ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif";
            // only accept MP4 for video uploads per requirement
            bool isVideo = contentType == "video/mp4" || ext == ".mp4";

            if (!isImage && !isVideo)
            {
                ltMaterialDescription.Text = "File type not supported. Upload an image or video.";
                return;
            }

            string folder = Server.MapPath($"~/assets/course_materials/{courseId}");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Path.GetFileName(fuMedia.FileName);
            string savePath = Path.Combine(folder, fileName);
            string relativePath = $"/assets/course_materials/{courseId}/{fileName}";

            try
            {
                fuMedia.SaveAs(savePath);

                // Insert into MaterialTable: prefer storing file path in materialType column
                string connStr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var con = new SqlConnection(connStr))
                {
                    con.Open();

                    var cols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    using (var schCmd = new SqlCommand("SELECT TOP 1 * FROM MaterialTable", con))
                    using (var da = new SqlDataAdapter(schCmd))
                    {
                        var sch = new DataTable();
                        da.Fill(sch);
                        foreach (DataColumn c in sch.Columns) cols.Add(c.ColumnName);
                    }

                    var insertCols = new List<string>();
                    var insertParams = new List<string>();
                    var parameters = new List<SqlParameter>();

                    if (cols.Contains("courseID"))
                    {
                        insertCols.Add("courseID");
                        insertParams.Add("@courseID");
                        parameters.Add(new SqlParameter("@courseID", SqlDbType.Int) { Value = Convert.ToInt32(courseId) });
                    }

                    // materialDescription removed from upload UI; do not attempt to read nonexistent control

                    // If a Thumbnail_img column exists, save the uploaded file path there as well
                    if (cols.Contains("Thumbnail_img"))
                    {
                        insertCols.Add("Thumbnail_img");
                        insertParams.Add("@Thumbnail_img");
                        parameters.Add(new SqlParameter("@Thumbnail_img", SqlDbType.NVarChar) { Value = relativePath });
                    }

                    // Prefer to store uploaded file path in materialType
                    if (cols.Contains("materialType"))
                    {
                        insertCols.Add("materialType");
                        insertParams.Add("@materialType");
                        parameters.Add(new SqlParameter("@materialType", SqlDbType.NVarChar) { Value = relativePath });
                    }
                    else if (cols.Contains("MediaPath"))
                    {
                        insertCols.Add("MediaPath");
                        insertParams.Add("@MediaPath");
                        parameters.Add(new SqlParameter("@MediaPath", SqlDbType.NVarChar) { Value = relativePath });
                    }
                    else if (cols.Contains("Image") && isImage)
                    {
                        insertCols.Add("Image");
                        insertParams.Add("@Image");
                        parameters.Add(new SqlParameter("@Image", SqlDbType.NVarChar) { Value = relativePath });
                    }
                    else if (cols.Contains("VideoPath") && isVideo)
                    {
                        insertCols.Add("VideoPath");
                        insertParams.Add("@VideoPath");
                        parameters.Add(new SqlParameter("@VideoPath", SqlDbType.NVarChar) { Value = relativePath });
                    }
                    else if (!cols.Contains("materialType") && !cols.Contains("MediaPath") && !cols.Contains("Image") && !cols.Contains("VideoPath"))
                    {
                        // fallback to FilePath
                        if (cols.Contains("FilePath"))
                        {
                            insertCols.Add("FilePath");
                            insertParams.Add("@FilePath");
                            parameters.Add(new SqlParameter("@FilePath", SqlDbType.NVarChar) { Value = relativePath });
                        }
                    }

                    if (insertCols.Count > 0)
                    {
                        string sql = $"INSERT INTO MaterialTable ({string.Join(",", insertCols)}) VALUES ({string.Join(",", insertParams)})";
                        using (var cmd = new SqlCommand(sql, con))
                        {
                            cmd.Parameters.AddRange(parameters.ToArray());
                            cmd.ExecuteNonQuery();
                        }

                        // Also update CourseDetailTable.Thumbnail_img for this course to point to the uploaded image (if column exists)
                        try
                        {
                            if (isImage && !string.IsNullOrEmpty(courseId))
                            {
                                var courseCols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                                using (var schCmd2 = new SqlCommand("SELECT TOP 1 * FROM CourseDetailTable", con))
                                using (var da2 = new SqlDataAdapter(schCmd2))
                                {
                                    var sch2 = new DataTable();
                                    da2.Fill(sch2);
                                    foreach (DataColumn c in sch2.Columns) courseCols.Add(c.ColumnName);
                                }

                                if (courseCols.Contains("Thumbnail_img"))
                                {
                                    using (var upd = new SqlCommand("UPDATE CourseDetailTable SET Thumbnail_img = @thumb WHERE courseID = @id", con))
                                    {
                                        upd.Parameters.AddWithValue("@thumb", relativePath);
                                        upd.Parameters.AddWithValue("@id", courseId);
                                        upd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // non-critical: don't block upload on thumbnail update failure
                        }

                        // If an image was uploaded, show it immediately in the preview area
                        if (isImage)
                        {
                            var imgUrl = ResolveUrl("~" + (relativePath.StartsWith("/") ? relativePath : "/" + relativePath));
                            imgCourseMedia.ImageUrl = imgUrl;
                            imgCourseMedia.Visible = true;
                            imgCourseMedia.Style["display"] = "block";
                            courseVID.Style["display"] = "none";
                            // ensure client-side preview (FileReader) doesn't conflict: clear videoSource
                            try { videoSource.Attributes["src"] = ""; } catch { }
                        }
                    }
                }

                // Refresh view
                LoadCourseAndMaterial(courseId);
            }
            catch (Exception ex)
            {
                ltMaterialDescription.Text = "Upload failed: " + HttpUtility.HtmlEncode(ex.Message);
            }
        }

        protected void btnEditTitle_Click(object sender, EventArgs e)
        {
            // Show server-side inline edit controls instead of client prompt
            pnlEditTitle.Visible = true;
            // populate the panel textbox if present
            var tb = pnlEditTitle.FindControl("txtTitleEdit") as TextBox;
            if (tb != null) tb.Text = HttpUtility.HtmlDecode(ltCourseTitle.Text ?? string.Empty);
        }

        protected void btnSaveTitle_Click(object sender, EventArgs e)
        {
            string newTitle = string.Empty;
            var tb = pnlEditTitle?.FindControl("txtTitleEdit") as TextBox;
            if (tb != null && !string.IsNullOrWhiteSpace(tb.Text))
            {
                newTitle = tb.Text.Trim();
            }
            else
            {
                newTitle = (hfNewCourseTitle.Value ?? string.Empty).Trim();
            }

            if (string.IsNullOrEmpty(newTitle)) return;

            string courseId = Request.QueryString["courseId"];
            string connStr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            try
            {
                using (var con = new SqlConnection(connStr))
                {
                    con.Open();
                    
                    var courseCols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    using (var schCmd = new SqlCommand("SELECT TOP 1 * FROM CourseDetailTable", con))
                    using (var daCols = new SqlDataAdapter(schCmd))
                    {
                        var sch = new DataTable();
                        daCols.Fill(sch);
                        foreach (DataColumn c in sch.Columns) courseCols.Add(c.ColumnName);
                    }

                    var sets = new List<string>();
                    var insertCols = new List<string>();
                    var insertParams = new List<string>();
                    var parms = new List<SqlParameter>();

                    // always set title if present
                    if (courseCols.Contains("courseTitle"))
                    {
                        sets.Add("courseTitle = @title");
                        insertCols.Add("courseTitle");
                        insertParams.Add("@title");
                        parms.Add(new SqlParameter("@title", SqlDbType.NVarChar) { Value = newTitle });
                    }

                    if (courseCols.Contains("Category"))
                    {
                        sets.Add("Category = @category");
                        insertCols.Add("Category");
                        insertParams.Add("@category");
                        parms.Add(new SqlParameter("@category", SqlDbType.NVarChar) { Value = (ddlCategory?.SelectedValue ?? string.Empty) });
                    }

                    if (courseCols.Contains("level"))
                    {
                        sets.Add("[level] = @level");
                        insertCols.Add("[level]");
                        insertParams.Add("@level");
                        parms.Add(new SqlParameter("@level", SqlDbType.NVarChar) { Value = (ddlLevel?.SelectedValue ?? string.Empty) });
                    }

                    if (courseCols.Contains("Instructor"))
                    {
                        sets.Add("Instructor = @instructor");
                        insertCols.Add("Instructor");
                        insertParams.Add("@instructor");
                        parms.Add(new SqlParameter("@instructor", SqlDbType.NVarChar) { Value = (txtInstructorEdit?.Text ?? string.Empty) });
                    }

                    if (courseCols.Contains("Price"))
                    {
                        sets.Add("Price = @price");
                        insertCols.Add("Price");
                        insertParams.Add("@price");
                        parms.Add(new SqlParameter("@price", SqlDbType.NVarChar) { Value = (txtPriceEdit?.Text ?? string.Empty) });
                    }

                    if (!string.IsNullOrEmpty(courseId))
                    {
                        if (sets.Count == 0)
                        {
                            // nothing to update, just return
                        }
                        else
                        {
                            string sql = $"UPDATE CourseDetailTable SET {string.Join(",", sets)} WHERE courseID = @id";
                            using (var cmd = new SqlCommand(sql, con))
                            {
                                cmd.Parameters.AddRange(parms.ToArray());
                                cmd.Parameters.AddWithValue("@id", courseId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    else
                    {
                        // insert new row using available columns
                        if (insertCols.Count > 0)
                        {
                            string sql = $"INSERT INTO CourseDetailTable ({string.Join(",", insertCols)}) VALUES ({string.Join(",", insertParams)}); SELECT SCOPE_IDENTITY();";
                            using (var cmd = new SqlCommand(sql, con))
                            {
                                cmd.Parameters.AddRange(parms.ToArray());
                                var obj = cmd.ExecuteScalar();
                                if (obj != null && int.TryParse(obj.ToString(), out var newId))
                                {
                                    // After creating a new course, return to the course list so admin can see it there.
                                    Response.Redirect("AdminManageCourse.aspx");
                                    return;
                                }
                            }
                        }
                    }
                }

                // Refresh the displayed course title
                ltCourseTitle.Text = HttpUtility.HtmlEncode(newTitle);

                // Refresh the category name displayed on the page
                string categoryText = string.IsNullOrEmpty(ddlCategory.SelectedValue)
                    ? string.Empty
                    : ddlCategory.SelectedItem.Text;

                ltCategory.Text = string.IsNullOrEmpty(categoryText)
                    ? string.Empty
                    : $"<span class=\"tag-pill\">{HttpUtility.HtmlEncode(categoryText)}</span>";

                // Refresh the displayed level
                string levelText = string.IsNullOrEmpty(ddlLevel.SelectedValue)
                    ? string.Empty
                    : ddlLevel.SelectedItem.Text;

                ltLevel.Text = string.IsNullOrEmpty(levelText)
                    ? string.Empty
                    : $"<span class=\"tag-pill\">{HttpUtility.HtmlEncode(levelText)}</span>";

                // Refresh the displayed instructor
                string instructorText = (txtInstructorEdit.Text ?? string.Empty).Trim();

                ltInstructor.Text = string.IsNullOrEmpty(instructorText)
                    ? string.Empty
                    : $"<span class=\"tag-pill\">{HttpUtility.HtmlEncode(instructorText)}</span>";

                // Hide the edit panel after saving
                pnlEditTitle.Visible = false;
            }
            catch (Exception ex)
            {
                ltMaterialDescription.Text = "Error saving title: " + HttpUtility.HtmlEncode(ex.Message);
            }
        }

        protected void btnEditDescription_Click(object sender, EventArgs e)
        {
            pnlEditDescription.Visible = true;
            txtDescriptionEdit.Text = HttpUtility.HtmlDecode(ltMaterialDescription.Text ?? string.Empty);
        }

        protected void btnSaveDescription_Click(object sender, EventArgs e)
        {
            var newDesc = txtDescriptionEdit.Text.Trim();
            string courseId = Request.QueryString["courseId"];
            if (string.IsNullOrEmpty(courseId))
            {
                ltMaterialDescription.Text = "Missing course id.";
                return;
            }

            string connStr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            try
            {
                using (var con = new SqlConnection(connStr))
                {
                    con.Open();
                    // try to update latest material's description
                    using (var cmd = new SqlCommand("SELECT TOP 1 materialID FROM MaterialTable WHERE courseID = @id ORDER BY materialID DESC", con))
                    {
                        cmd.Parameters.AddWithValue("@id", courseId);
                        var res = cmd.ExecuteScalar();
                        if (res != null)
                        {
                            int matId = Convert.ToInt32(res);
                            using (var u = new SqlCommand("UPDATE MaterialTable SET materialDescription = @desc WHERE materialID = @mid", con))
                            {
                                u.Parameters.AddWithValue("@desc", (object)newDesc ?? DBNull.Value);
                                u.Parameters.AddWithValue("@mid", matId);
                                u.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            // insert new material row with description
                            using (var ins = new SqlCommand("INSERT INTO MaterialTable (courseID, materialDescription) VALUES (@id, @desc)", con))
                            {
                                ins.Parameters.AddWithValue("@id", courseId);
                                ins.Parameters.AddWithValue("@desc", (object)newDesc ?? DBNull.Value);
                                ins.ExecuteNonQuery();
                            }
                        }
                    }
                }

                ltMaterialDescription.Text = HttpUtility.HtmlEncode(newDesc);
                pnlEditDescription.Visible = false;
            }
            catch (Exception ex)
            {
                ltMaterialDescription.Text = "Error saving description: " + HttpUtility.HtmlEncode(ex.Message);
            }
        }
    }
}
