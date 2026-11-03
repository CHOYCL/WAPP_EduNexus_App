using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;

namespace WAPP_EduNexus_App
{
    public partial class ModuleLearn : System.Web.UI.Page
    {
        private int _userId;
        private int _selectedModuleId;

        protected int CourseId
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
            _userId = QuizHelper.RequireUser(this);

            if (!IsPostBack)
            {
                LoadPage();
            }
        }

        private void LoadPage()
        {
            if (CourseId == 0)
            {
                Block("Course not found.");
                return;
            }

            try
            {
                using (var con = new SqlConnection(QuizHelper.ConnStr))
                {
                    con.Open();

                    object title;
                    using (var cmd = new SqlCommand("SELECT courseTitle FROM dbo.CourseDetailTable WHERE courseID = @c", con))
                    {
                        cmd.Parameters.Add("@c", SqlDbType.Int).Value = CourseId;
                        title = cmd.ExecuteScalar();
                    }
                    if (title == null)
                    {
                        Block("Course not found.");
                        return;
                    }
                    ltCourseTitle.Text = HttpUtility.HtmlEncode(Convert.ToString(title));

                    // Only enrolled students can open the modules.
                    if (!QuizHelper.IsEnrolled(con, _userId, CourseId))
                    {
                        Block("Enroll in this course to open its modules and quizzes.");
                        return;
                    }

                    // Modules with this student's quiz status.
                    var modules = new DataTable();
                    using (var cmd = new SqlCommand(@"
                        SELECT m.moduleID, m.moduleTitle,
                               (SELECT COUNT(*) FROM dbo.QuizAttemptTable t
                                WHERE t.moduleID = m.moduleID AND t.UserID = @u) AS AttemptsUsed,
                               (SELECT COUNT(*) FROM dbo.QuizAttemptTable t
                                WHERE t.moduleID = m.moduleID AND t.UserID = @u AND t.isPassed = 1) AS PassedCount,
                               (SELECT COUNT(*) FROM dbo.QuizAttemptTable t
                                WHERE t.moduleID = m.moduleID AND t.UserID = @u AND t.status = 'Pending') AS PendingCount
                        FROM dbo.ModuleTable m
                        WHERE m.courseID = @c
                        ORDER BY m.sortOrder, m.moduleID", con))
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        cmd.Parameters.Add("@u", SqlDbType.Int).Value = _userId;
                        cmd.Parameters.Add("@c", SqlDbType.Int).Value = CourseId;
                        da.Fill(modules);
                    }

                    if (modules.Rows.Count == 0)
                    {
                        Block("This course has no modules yet. Please check back soon.");
                        return;
                    }

                    // Use the module in the address, or the first module.
                    int requested;
                    int.TryParse(Request.QueryString["moduleId"], out requested);
                    _selectedModuleId = modules.Select("moduleID = " + requested).Length > 0
                        ? requested
                        : Convert.ToInt32(modules.Rows[0]["moduleID"]);

                    rptModules.DataSource = modules;
                    rptModules.DataBind();

                    LoadModuleContent(con);
                }
            }
            catch (Exception)
            {
                Block("Unable to load this course. Please try again later.");
            }
        }

        private void LoadModuleContent(SqlConnection con)
        {
            int passPercentage;
            using (var cmd = new SqlCommand(
                "SELECT moduleTitle, moduleDescription, passPercentage FROM dbo.ModuleTable WHERE moduleID = @m", con))
            {
                cmd.Parameters.Add("@m", SqlDbType.Int).Value = _selectedModuleId;
                using (var r = cmd.ExecuteReader())
                {
                    r.Read();
                    ltModuleTitle.Text = HttpUtility.HtmlEncode(r.GetString(0));
                    ltModuleDesc.Text = r.IsDBNull(1) ? string.Empty : HttpUtility.HtmlEncode(r.GetString(1));
                    passPercentage = r.GetInt32(2);
                }
            }

            // Videos and readings in the order the admin set.
            var materials = new DataTable();
            using (var cmd = new SqlCommand(@"
                SELECT materialTitle, materialKind, filePath, readingContent
                FROM dbo.ModuleMaterialTable
                WHERE moduleID = @m
                ORDER BY sortOrder, moduleMaterialID", con))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add("@m", SqlDbType.Int).Value = _selectedModuleId;
                da.Fill(materials);
            }

            ltNoMaterials.Text = materials.Rows.Count == 0
                ? "<div class=\"qz-card qz-muted\">No learning material has been added to this module yet.</div>"
                : string.Empty;
            rptMaterials.DataSource = materials;
            rptMaterials.DataBind();

            LoadQuizBox(con, passPercentage);
        }

        private void LoadQuizBox(SqlConnection con, int passPercentage)
        {
            int questionCount, totalMarks;
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*), ISNULL(SUM(marks), 0) FROM dbo.QuizQuestionTable WHERE moduleID = @m", con))
            {
                cmd.Parameters.Add("@m", SqlDbType.Int).Value = _selectedModuleId;
                using (var r = cmd.ExecuteReader())
                {
                    r.Read();
                    questionCount = r.GetInt32(0);
                    totalMarks = r.GetInt32(1);
                }
            }

            if (questionCount == 0)
            {
                ltQuizInfo.Text = "<p class=\"qz-muted\">The quiz for this module is not ready yet.</p>";
                return;
            }

            AttemptState state = QuizHelper.GetAttemptState(con, _userId, _selectedModuleId);

            var info = new StringBuilder();
            info.Append("<p>" + questionCount + " question(s), " + totalMarks + " marks. Pass mark: " + passPercentage + "%.</p>");
            info.Append("<p class=\"qz-muted\">Attempts used: " + state.Used + " of " + QuizHelper.MaxAttempts + ".</p>");

            if (state.Passed)
                info.Append("<p class=\"qz-pass\">You passed this quiz.</p>");
            else if (state.Pending)
                info.Append("<p class=\"qz-muted\">Your latest attempt is waiting to be marked.</p>");
            else if (state.Used >= QuizHelper.MaxAttempts)
                info.Append("<p class=\"qz-fail\">You have used all your attempts.</p>");

            ltQuizInfo.Text = info.ToString();

            if (state.CanAttempt)
            {
                lnkStartQuiz.Text = state.Used == 0
                    ? "Start quiz"
                    : "Retake quiz (attempt " + (state.Used + 1) + " of " + QuizHelper.MaxAttempts + ")";
                lnkStartQuiz.NavigateUrl = "TakeQuiz.aspx?moduleId=" + _selectedModuleId;
                lnkStartQuiz.Visible = true;
            }

            if (state.LatestAttemptId > 0)
            {
                lnkViewResult.NavigateUrl = "QuizResult.aspx?attemptId=" + state.LatestAttemptId;
                lnkViewResult.Visible = true;
            }
        }

        // ---------------------------------------------------------------
        // Markup helpers
        // ---------------------------------------------------------------

        protected string IsActive(object moduleId)
        {
            return Convert.ToInt32(moduleId) == _selectedModuleId ? "active" : string.Empty;
        }

        protected string StatusBadge(object attemptsUsed, object passedCount, object pendingCount)
        {
            int used = Convert.ToInt32(attemptsUsed);

            if (Convert.ToInt32(passedCount) > 0)
                return "<span class=\"qz-badge qz-badge-pass\">Passed</span>";
            if (Convert.ToInt32(pendingCount) > 0)
                return "<span class=\"qz-badge qz-badge-wait\">Marking</span>";
            if (used >= QuizHelper.MaxAttempts)
                return "<span class=\"qz-badge qz-badge-fail\">Failed</span>";
            if (used > 0)
                return "<span class=\"qz-badge\">" + used + "/" + QuizHelper.MaxAttempts + "</span>";
            return string.Empty;
        }

        /// <summary>Builds the HTML for one video or reading item (all text is encoded).</summary>
        protected string RenderMaterial(object item)
        {
            var row = (DataRowView)item;
            string kind = Convert.ToString(row["materialKind"]);
            string title = HttpUtility.HtmlEncode(Convert.ToString(row["materialTitle"]));
            string path = Convert.ToString(row["filePath"]);
            string text = Convert.ToString(row["readingContent"]);
            string url = path.Length == 0 ? string.Empty : HttpUtility.HtmlAttributeEncode(ResolveUrl("~" + path));

            var html = new StringBuilder();
            html.Append("<div class=\"qz-card\"><h3 class=\"qz-card-title\">" + title + "</h3>");

            if (kind == "Video")
            {
                if (url.Length > 0)
                {
                    html.Append("<video class=\"qz-video\" controls preload=\"metadata\">" +
                                "<source src=\"" + url + "\" type=\"video/mp4\" />" +
                                "Your browser does not support the video tag.</video>");
                }
            }
            else
            {
                if (text.Length > 0)
                {
                    html.Append("<div class=\"qz-reading\">" + HttpUtility.HtmlEncode(text) + "</div>");
                }
                if (url.Length > 0)
                {
                    html.Append("<p><a class=\"qz-link\" href=\"" + url + "\" target=\"_blank\" rel=\"noopener\">Open the PDF</a></p>");
                }
            }

            html.Append("</div>");
            return html.ToString();
        }

        private void Block(string message)
        {
            lblMessage.Text = HttpUtility.HtmlEncode(message);
            lblMessage.CssClass = "qz-msg qz-msg-err";
            lblMessage.Visible = true;
            pnlLearn.Visible = false;
        }
    }
}