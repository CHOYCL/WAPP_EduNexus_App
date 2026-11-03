using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI.WebControls;

namespace WAPP_EduNexus_App
{
    public partial class AdminManageMaterial : System.Web.UI.Page
    {
        private int ModuleId
        {
            get
            {
                int id;
                int.TryParse(Request.QueryString["moduleId"], out id);
                return id;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            QuizHelper.RequireAdmin(this);

            if (ModuleId == 0)
            {
                Response.Redirect("AdminManageCourse.aspx", true);
            }

            if (!IsPostBack)
            {
                LoadHeader();
                ResetForm();
                LoadMaterials();
            }
        }

        // ---------------------------------------------------------------
        // Display
        // ---------------------------------------------------------------

        private void LoadHeader()
        {
            using (var con = new SqlConnection(QuizHelper.ConnStr))
            using (var cmd = new SqlCommand("SELECT moduleTitle, courseID FROM dbo.ModuleTable WHERE moduleID = @m", con))
            {
                cmd.Parameters.Add("@m", SqlDbType.Int).Value = ModuleId;
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read())
                    {
                        Response.Redirect("AdminManageCourse.aspx", true);
                    }
                    ltModuleTitle.Text = HttpUtility.HtmlEncode(r.GetString(0));
                    lnkBack.NavigateUrl = "AdminManageModule.aspx?courseId=" + r.GetInt32(1);
                }
            }
        }

        private void LoadMaterials()
        {
            var dt = new DataTable();
            using (var con = new SqlConnection(QuizHelper.ConnStr))
            using (var cmd = new SqlCommand(@"
                SELECT moduleMaterialID, materialTitle, materialKind, filePath, readingContent, sortOrder
                FROM dbo.ModuleMaterialTable
                WHERE moduleID = @m
                ORDER BY sortOrder, moduleMaterialID", con))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add("@m", SqlDbType.Int).Value = ModuleId;
                da.Fill(dt);
            }

            rptMaterials.Visible = dt.Rows.Count > 0;
            ltEmpty.Text = dt.Rows.Count == 0
                ? "<p class=\"qz-muted\">No materials yet. Add a video or reading material above.</p>"
                : string.Empty;
            rptMaterials.DataSource = dt;
            rptMaterials.DataBind();
        }

        /// <summary>Short description of what a material contains, shown in the list.</summary>
        protected string Summary(object item)
        {
            var row = (DataRowView)item;
            string path = Convert.ToString(row["filePath"]);
            string text = Convert.ToString(row["readingContent"]);

            string result = string.Empty;
            if (path.Length > 0) result = Path.GetFileName(path);
            if (text.Length > 0) result += (result.Length > 0 ? " + " : string.Empty) + "typed text (" + text.Length + " characters)";
            return result.Length == 0 ? "-" : result;
        }

        // ---------------------------------------------------------------
        // Create and update
        // ---------------------------------------------------------------

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            int sortOrder, materialId;
            if (!int.TryParse(txtSortOrder.Text.Trim(), out sortOrder) || sortOrder < 1)
            {
                ShowMessage("Order must be a whole number from 1.", false);
                return;
            }

            bool editing = int.TryParse(hfMaterialId.Value, out materialId) && materialId > 0;
            string kind = ddlKind.SelectedValue;
            string reading = txtReading.Text.Trim();
            string ext = fuFile.HasFile ? Path.GetExtension(fuFile.FileName).ToLowerInvariant() : string.Empty;

            // Validate the file type and the "something to show" rule.
            if (fuFile.HasFile)
            {
                bool okType = kind == "Video" ? ext == ".mp4" : ext == ".pdf";
                if (!okType)
                {
                    ShowMessage(kind == "Video" ? "Videos must be MP4 files." : "Reading files must be PDF files.", false);
                    return;
                }
            }

            if (!editing)
            {
                if (kind == "Video" && !fuFile.HasFile)
                {
                    ShowMessage("Choose an MP4 video to upload.", false);
                    return;
                }
                if (kind == "Reading" && !fuFile.HasFile && reading.Length == 0)
                {
                    ShowMessage("Type the reading text or upload a PDF.", false);
                    return;
                }
            }

            try
            {
                // Save the uploaded file (if any) under /assets/module_materials/{moduleId}/
                string newPath = null;
                if (fuFile.HasFile)
                {
                    string folder = Server.MapPath("~/assets/module_materials/" + ModuleId);
                    Directory.CreateDirectory(folder);

                    string safeName = Regex.Replace(Path.GetFileNameWithoutExtension(fuFile.FileName), @"[^A-Za-z0-9_\-]", "_") + ext;
                    string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + safeName;
                    fuFile.SaveAs(Path.Combine(folder, fileName));
                    newPath = "/assets/module_materials/" + ModuleId + "/" + fileName;
                }

                string oldPath = null;

                using (var con = new SqlConnection(QuizHelper.ConnStr))
                {
                    con.Open();

                    if (editing && newPath != null)
                    {
                        using (var cmd = new SqlCommand(
                            "SELECT filePath FROM dbo.ModuleMaterialTable WHERE moduleMaterialID = @id AND moduleID = @m", con))
                        {
                            cmd.Parameters.Add("@id", SqlDbType.Int).Value = materialId;
                            cmd.Parameters.Add("@m", SqlDbType.Int).Value = ModuleId;
                            oldPath = Convert.ToString(cmd.ExecuteScalar());
                        }
                    }

                    string sql = editing
                        ? @"UPDATE dbo.ModuleMaterialTable
                            SET materialTitle = @t, readingContent = @r, sortOrder = @s, filePath = ISNULL(@f, filePath)
                            WHERE moduleMaterialID = @id AND moduleID = @m"
                        : @"INSERT INTO dbo.ModuleMaterialTable (moduleID, materialTitle, materialKind, filePath, readingContent, sortOrder)
                            VALUES (@m, @t, @k, @f, @r, @s)";

                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.Add("@m", SqlDbType.Int).Value = ModuleId;
                        cmd.Parameters.Add("@t", SqlDbType.NVarChar, 150).Value = txtTitle.Text.Trim();
                        cmd.Parameters.Add("@f", SqlDbType.NVarChar, 500).Value = (object)newPath ?? DBNull.Value;
                        cmd.Parameters.Add("@r", SqlDbType.NVarChar, -1).Value =
                            (kind == "Reading" && reading.Length > 0) ? (object)reading : DBNull.Value;
                        cmd.Parameters.Add("@s", SqlDbType.Int).Value = sortOrder;
                        if (editing) cmd.Parameters.Add("@id", SqlDbType.Int).Value = materialId;
                        else cmd.Parameters.Add("@k", SqlDbType.NVarChar, 10).Value = kind;
                        cmd.ExecuteNonQuery();
                    }
                }

                if (!string.IsNullOrEmpty(oldPath)) DeleteFileQuietly(oldPath);

                ShowMessage(editing ? "Material updated." : "Material added.", true);
                ResetForm();
                LoadMaterials();
            }
            catch (Exception)
            {
                ShowMessage("Unable to save the material. Check the file size and try again.", false);
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ResetForm();
        }

        // ---------------------------------------------------------------
        // Edit and delete (Repeater buttons)
        // ---------------------------------------------------------------

        protected void rptMaterials_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int materialId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out materialId)) return;

            if (e.CommandName == "EditMaterial")
            {
                LoadMaterialIntoForm(materialId);
            }
            else if (e.CommandName == "DeleteMaterial")
            {
                DeleteMaterial(materialId);
            }
        }

        private void LoadMaterialIntoForm(int materialId)
        {
            using (var con = new SqlConnection(QuizHelper.ConnStr))
            using (var cmd = new SqlCommand(@"
                SELECT materialTitle, materialKind, readingContent, sortOrder
                FROM dbo.ModuleMaterialTable
                WHERE moduleMaterialID = @id AND moduleID = @m", con))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = materialId;
                cmd.Parameters.Add("@m", SqlDbType.Int).Value = ModuleId;
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read()) return;
                    txtTitle.Text = r.GetString(0);
                    ddlKind.SelectedValue = r.GetString(1);
                    txtReading.Text = r.IsDBNull(2) ? string.Empty : r.GetString(2);
                    txtSortOrder.Text = r.GetInt32(3).ToString();
                }
            }

            hfMaterialId.Value = materialId.ToString();
            ddlKind.Enabled = false;   // the type cannot change after the material is created
            ltFormTitle.Text = "Edit material";
            btnSave.Text = "Save changes";
            btnCancel.Visible = true;
        }

        private void DeleteMaterial(int materialId)
        {
            try
            {
                string path;
                using (var con = new SqlConnection(QuizHelper.ConnStr))
                {
                    con.Open();

                    using (var cmd = new SqlCommand(
                        "SELECT filePath FROM dbo.ModuleMaterialTable WHERE moduleMaterialID = @id AND moduleID = @m", con))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = materialId;
                        cmd.Parameters.Add("@m", SqlDbType.Int).Value = ModuleId;
                        path = Convert.ToString(cmd.ExecuteScalar());
                    }

                    using (var cmd = new SqlCommand(
                        "DELETE FROM dbo.ModuleMaterialTable WHERE moduleMaterialID = @id AND moduleID = @m", con))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = materialId;
                        cmd.Parameters.Add("@m", SqlDbType.Int).Value = ModuleId;
                        cmd.ExecuteNonQuery();
                    }
                }

                if (!string.IsNullOrEmpty(path)) DeleteFileQuietly(path);

                if (hfMaterialId.Value == materialId.ToString()) ResetForm();
                ShowMessage("Material deleted.", true);
                LoadMaterials();
            }
            catch (Exception)
            {
                ShowMessage("Unable to delete the material. Please try again.", false);
            }
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private void DeleteFileQuietly(string relativePath)
        {
            try
            {
                string fullPath = Server.MapPath("~" + relativePath);
                if (File.Exists(fullPath)) File.Delete(fullPath);
            }
            catch
            {
                // Not critical: the database row is already updated.
            }
        }

        private void ResetForm()
        {
            hfMaterialId.Value = string.Empty;
            txtTitle.Text = string.Empty;
            txtReading.Text = string.Empty;
            txtSortOrder.Text = "1";
            ddlKind.SelectedIndex = 0;
            ddlKind.Enabled = true;
            ltFormTitle.Text = "Add a material";
            btnSave.Text = "Add material";
            btnCancel.Visible = false;
        }

        private void ShowMessage(string message, bool success)
        {
            lblMessage.Text = HttpUtility.HtmlEncode(message);
            lblMessage.CssClass = success ? "qz-msg qz-msg-ok" : "qz-msg qz-msg-err";
            lblMessage.Visible = true;
        }
    }
}
