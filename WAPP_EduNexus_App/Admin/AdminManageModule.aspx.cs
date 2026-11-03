using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI.WebControls;

namespace WAPP_EduNexus_App
{
    public partial class AdminManageModule : System.Web.UI.Page
    {
        private int CourseId
        {
            get
            {
                int id;
                int.TryParse(Request.QueryString["courseId"], out id);
                return id;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            QuizHelper.RequireAdmin(this);

            if (CourseId == 0)
            {
                Response.Redirect("AdminManageCourse.aspx", true);
            }

            if (!IsPostBack)
            {
                LoadCourseTitle();
                ResetForm();
                LoadModules();
            }
        }

        // ---------------------------------------------------------------
        // Display
        // ---------------------------------------------------------------

        private void LoadCourseTitle()
        {
            using (var con = new SqlConnection(QuizHelper.ConnStr))
            using (var cmd = new SqlCommand("SELECT courseTitle FROM dbo.CourseDetailTable WHERE courseID = @c", con))
            {
                cmd.Parameters.Add("@c", SqlDbType.Int).Value = CourseId;
                con.Open();
                object title = cmd.ExecuteScalar();
                if (title == null)
                {
                    Response.Redirect("AdminManageCourse.aspx", true);
                }
                ltCourseTitle.Text = HttpUtility.HtmlEncode(Convert.ToString(title));
            }
        }

        private void LoadModules()
        {
            var dt = new DataTable();
            using (var con = new SqlConnection(QuizHelper.ConnStr))
            using (var cmd = new SqlCommand(@"
                SELECT m.moduleID, m.moduleTitle, m.sortOrder, m.passPercentage,
                       (SELECT COUNT(*) FROM dbo.ModuleMaterialTable x WHERE x.moduleID = m.moduleID) AS MaterialCount,
                       (SELECT COUNT(*) FROM dbo.QuizQuestionTable q WHERE q.moduleID = m.moduleID) AS QuestionCount
                FROM dbo.ModuleTable m
                WHERE m.courseID = @c
                ORDER BY m.sortOrder, m.moduleID", con))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add("@c", SqlDbType.Int).Value = CourseId;
                da.Fill(dt);
            }

            rptModules.Visible = dt.Rows.Count > 0;
            ltEmpty.Text = dt.Rows.Count == 0
                ? "<p class=\"qz-muted\">No modules yet. Add the first one above.</p>"
                : string.Empty;
            rptModules.DataSource = dt;
            rptModules.DataBind();
        }

        // ---------------------------------------------------------------
        // Create and update
        // ---------------------------------------------------------------

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            int sortOrder, passPercentage, moduleId;
            if (!int.TryParse(txtSortOrder.Text.Trim(), out sortOrder) ||
                !int.TryParse(txtPassPercentage.Text.Trim(), out passPercentage) ||
                sortOrder < 1 || passPercentage < 1 || passPercentage > 100)
            {
                ShowMessage("Order must be a whole number from 1, and the pass mark from 1 to 100.", false);
                return;
            }

            bool editing = int.TryParse(hfModuleId.Value, out moduleId) && moduleId > 0;
            string description = txtDescription.Text.Trim();

            string sql = editing
                ? @"UPDATE dbo.ModuleTable
                    SET moduleTitle = @t, moduleDescription = @d, sortOrder = @s, passPercentage = @p
                    WHERE moduleID = @id AND courseID = @c"
                : @"INSERT INTO dbo.ModuleTable (courseID, moduleTitle, moduleDescription, sortOrder, passPercentage)
                    VALUES (@c, @t, @d, @s, @p)";

            try
            {
                using (var con = new SqlConnection(QuizHelper.ConnStr))
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.Add("@c", SqlDbType.Int).Value = CourseId;
                    cmd.Parameters.Add("@t", SqlDbType.NVarChar, 150).Value = txtTitle.Text.Trim();
                    cmd.Parameters.Add("@d", SqlDbType.NVarChar, -1).Value =
                        description.Length == 0 ? (object)DBNull.Value : description;
                    cmd.Parameters.Add("@s", SqlDbType.Int).Value = sortOrder;
                    cmd.Parameters.Add("@p", SqlDbType.Int).Value = passPercentage;
                    if (editing) cmd.Parameters.Add("@id", SqlDbType.Int).Value = moduleId;

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                ShowMessage(editing ? "Module updated." : "Module added.", true);
                ResetForm();
                LoadModules();
            }
            catch (Exception)
            {
                ShowMessage("Unable to save the module. Please try again.", false);
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ResetForm();
        }

        // ---------------------------------------------------------------
        // Edit and delete (Repeater buttons)
        // ---------------------------------------------------------------

        protected void rptModules_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int moduleId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out moduleId)) return;

            if (e.CommandName == "EditModule")
            {
                LoadModuleIntoForm(moduleId);
            }
            else if (e.CommandName == "DeleteModule")
            {
                DeleteModule(moduleId);
            }
        }

        private void LoadModuleIntoForm(int moduleId)
        {
            using (var con = new SqlConnection(QuizHelper.ConnStr))
            using (var cmd = new SqlCommand(@"
                SELECT moduleTitle, moduleDescription, sortOrder, passPercentage
                FROM dbo.ModuleTable
                WHERE moduleID = @id AND courseID = @c", con))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = moduleId;
                cmd.Parameters.Add("@c", SqlDbType.Int).Value = CourseId;
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read()) return;
                    txtTitle.Text = r.GetString(0);
                    txtDescription.Text = r.IsDBNull(1) ? string.Empty : r.GetString(1);
                    txtSortOrder.Text = r.GetInt32(2).ToString();
                    txtPassPercentage.Text = r.GetInt32(3).ToString();
                }
            }

            hfModuleId.Value = moduleId.ToString();
            ltFormTitle.Text = "Edit module";
            btnSave.Text = "Save changes";
            btnCancel.Visible = true;
        }

        private void DeleteModule(int moduleId)
        {
            try
            {
                using (var con = new SqlConnection(QuizHelper.ConnStr))
                {
                    con.Open();
                    using (var tx = con.BeginTransaction())
                    {
                        try
                        {
                            // Remove student answers first, then the module (questions, materials and attempts cascade).
                            using (var cmd = new SqlCommand(@"
                                DELETE a
                                FROM dbo.QuizAnswerTable a
                                INNER JOIN dbo.QuizQuestionTable q ON q.questionID = a.questionID
                                WHERE q.moduleID = @id", con, tx))
                            {
                                cmd.Parameters.Add("@id", SqlDbType.Int).Value = moduleId;
                                cmd.ExecuteNonQuery();
                            }

                            using (var cmd = new SqlCommand(
                                "DELETE FROM dbo.ModuleTable WHERE moduleID = @id AND courseID = @c", con, tx))
                            {
                                cmd.Parameters.Add("@id", SqlDbType.Int).Value = moduleId;
                                cmd.Parameters.Add("@c", SqlDbType.Int).Value = CourseId;
                                cmd.ExecuteNonQuery();
                            }

                            tx.Commit();
                        }
                        catch
                        {
                            try { tx.Rollback(); } catch { }
                            throw;
                        }
                    }
                }

                // Delete the uploaded files too.
                try
                {
                    string folder = Server.MapPath("~/assets/module_materials/" + moduleId);
                    if (System.IO.Directory.Exists(folder)) System.IO.Directory.Delete(folder, true);
                }
                catch
                {
                    // Not critical: the database rows are already gone.
                }

                if (hfModuleId.Value == moduleId.ToString()) ResetForm();
                ShowMessage("Module deleted.", true);
                LoadModules();
            }
            catch (Exception)
            {
                ShowMessage("Unable to delete the module. Please try again.", false);
            }
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private void ResetForm()
        {
            hfModuleId.Value = string.Empty;
            txtTitle.Text = string.Empty;
            txtDescription.Text = string.Empty;
            txtSortOrder.Text = "1";
            txtPassPercentage.Text = "50";
            ltFormTitle.Text = "Add a module";
            btnSave.Text = "Add module";
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
