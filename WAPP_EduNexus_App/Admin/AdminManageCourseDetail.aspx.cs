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
                string courseId = Request.QueryString["courseId"];
                if (!string.IsNullOrEmpty(courseId))
                {
                    LoadCourseAndMaterial(courseId);
                }
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

                    // Load course title, category, level, instructor
                    using (var cmd = new SqlCommand("SELECT TOP 1 courseTitle, Category, level, Instructor FROM CourseDetailTable WHERE courseID = @id", con))
                    {
                        cmd.Parameters.AddWithValue("@id", courseId);
                        using (var rdr = cmd.ExecuteReader())
                        {
                            if (rdr.Read())
                            {
                                ltCourseTitle.Text = HttpUtility.HtmlEncode(Convert.ToString(rdr["courseTitle"] ?? string.Empty));
                                var cat = Convert.ToString(rdr["Category"] ?? string.Empty);
                                var lvl = Convert.ToString(rdr["level"] ?? string.Empty);
                                var instr = Convert.ToString(rdr["Instructor"] ?? string.Empty);

                                ltCategory.Text = string.IsNullOrEmpty(cat) ? string.Empty : $"<span class=\"tag-pill\">{HttpUtility.HtmlEncode(cat)}</span>";
                                ltLevel.Text = string.IsNullOrEmpty(lvl) ? string.Empty : $"<span class=\"tag-pill\">{HttpUtility.HtmlEncode(lvl)}</span>";
                                ltInstructor.Text = string.IsNullOrEmpty(instr) ? string.Empty : $"<span class=\"tag-pill\">{HttpUtility.HtmlEncode(instr)}</span>";
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
            // Prefer server-side textbox value when using inline panel edit, fallback to hidden field used by legacy prompt flow
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
                    if (!string.IsNullOrEmpty(courseId))
                    {
                        using (var cmd = new SqlCommand("UPDATE CourseDetailTable SET courseTitle = @title, Category = @category, [level] = @level, Instructor = @instructor WHERE courseID = @id", con))
                        {
                            cmd.Parameters.AddWithValue("@title", newTitle);
                            cmd.Parameters.AddWithValue("@id", courseId);
                            cmd.Parameters.AddWithValue("@category", (ddlCategory?.SelectedValue ?? string.Empty));
                            cmd.Parameters.AddWithValue("@level", (ddlLevel?.SelectedValue ?? string.Empty));
                            cmd.Parameters.AddWithValue("@instructor", (txtInstructorEdit?.Text ?? string.Empty));
                            cmd.Parameters.AddWithValue("@price", (txtPriceEdit?.Text ?? string.Empty));
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        // Some CourseDetailTable columns (e.g. level) do not allow NULL. Provide sensible defaults when creating a new row.
                        using (var cmd = new SqlCommand("INSERT INTO CourseDetailTable (courseTitle, [level], Category, Instructor) VALUES (@title, @level, @category, @instructor); SELECT SCOPE_IDENTITY();", con))
                        {
                            cmd.Parameters.AddWithValue("@title", newTitle);
                            cmd.Parameters.AddWithValue("@level", (ddlLevel?.SelectedValue ?? string.Empty));
                            cmd.Parameters.AddWithValue("@category", (ddlCategory?.SelectedValue ?? string.Empty));
                            cmd.Parameters.AddWithValue("@instructor", (txtInstructorEdit?.Text ?? string.Empty));
                            cmd.Parameters.AddWithValue("@price", (txtPriceEdit?.Text ?? string.Empty));
                            var obj = cmd.ExecuteScalar();
                            if (obj != null && int.TryParse(obj.ToString(), out var newId))
                            {
                                Response.Redirect("AdminManageCourseDetail.aspx?courseId=" + newId);
                                return;
                            }
                        }
                    }
                }

                // refresh displayed title
                ltCourseTitle.Text = HttpUtility.HtmlEncode(newTitle);
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
