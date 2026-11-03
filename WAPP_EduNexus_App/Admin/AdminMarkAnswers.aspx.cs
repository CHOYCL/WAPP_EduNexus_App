using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web;
using System.Web.UI.WebControls;

namespace WAPP_EduNexus_App
{
    public partial class AdminMarkAnswers : System.Web.UI.Page
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

        private int AttemptId
        {
            get
            {
                int id;
                int.TryParse(Request.QueryString["attemptId"], out id);
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
                LoadAttempts();
                if (AttemptId > 0) LoadAnswers();
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

        private void LoadAttempts()
        {
            var dt = new DataTable();
            using (var con = new SqlConnection(QuizHelper.ConnStr))
            using (var cmd = new SqlCommand(@"
                SELECT t.attemptID, t.moduleID, u.Username, t.attemptNo, t.attemptDate, t.status,
                       t.score, t.totalMarks, t.percentage, t.grade
                FROM dbo.QuizAttemptTable t
                INNER JOIN dbo.Users u ON u.Id = t.UserID
                WHERE t.moduleID = @m
                ORDER BY CASE WHEN t.status = 'Pending' THEN 0 ELSE 1 END, t.attemptDate DESC", con))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add("@m", SqlDbType.Int).Value = ModuleId;
                da.Fill(dt);
            }

            rptAttempts.Visible = dt.Rows.Count > 0;
            ltAttemptsEmpty.Text = dt.Rows.Count == 0
                ? "<p class=\"qz-muted\">No student has attempted this quiz yet.</p>"
                : string.Empty;
            rptAttempts.DataSource = dt;
            rptAttempts.DataBind();
        }

        protected string Pct(object value)
        {
            if (value == null || value == DBNull.Value) return "-";
            return Convert.ToDecimal(value).ToString("0.##") + "%";
        }

        /// <summary>Shows the long answers of the selected attempt so the admin can award marks.</summary>
        private void LoadAnswers()
        {
            using (var con = new SqlConnection(QuizHelper.ConnStr))
            {
                con.Open();

                // Confirm the attempt belongs to this module and find who made it.
                string student;
                int attemptNo;
                using (var cmd = new SqlCommand(@"
                    SELECT u.Username, t.attemptNo
                    FROM dbo.QuizAttemptTable t
                    INNER JOIN dbo.Users u ON u.Id = t.UserID
                    WHERE t.attemptID = @a AND t.moduleID = @m", con))
                {
                    cmd.Parameters.Add("@a", SqlDbType.Int).Value = AttemptId;
                    cmd.Parameters.Add("@m", SqlDbType.Int).Value = ModuleId;
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read())
                        {
                            pnlMark.Visible = false;
                            ShowMessage("That attempt was not found for this module.", false);
                            return;
                        }
                        student = r.GetString(0);
                        attemptNo = r.GetInt32(1);
                    }
                }

                var dt = new DataTable();
                using (var cmd = new SqlCommand(@"
                    SELECT a.answerID, q.questionText, q.marks, q.modelAnswer, a.answerText,
                           ISNULL(CONVERT(VARCHAR(10), a.marksAwarded), '') AS AwardedText,
                           ISNULL(a.adminFeedback, '') AS FeedbackText
                    FROM dbo.QuizAnswerTable a
                    INNER JOIN dbo.QuizQuestionTable q ON q.questionID = a.questionID
                    WHERE a.attemptID = @a AND q.questionType = 'Long'
                    ORDER BY q.questionID", con))
                using (var da = new SqlDataAdapter(cmd))
                {
                    cmd.Parameters.Add("@a", SqlDbType.Int).Value = AttemptId;
                    da.Fill(dt);
                }

                pnlMark.Visible = true;
                ltMarkHeader.Text = "Marking: " + HttpUtility.HtmlEncode(student) + ", attempt " + attemptNo + " of " + QuizHelper.MaxAttempts;
                ltNoLong.Text = dt.Rows.Count == 0
                    ? "<p class=\"qz-muted\">This attempt has no long answers to mark.</p>"
                    : string.Empty;
                btnSaveMarks.Visible = dt.Rows.Count > 0;
                rptAnswers.DataSource = dt;
                rptAnswers.DataBind();
            }
        }

        // ---------------------------------------------------------------
        // Save marks
        // ---------------------------------------------------------------

        protected void btnSaveMarks_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                using (var con = new SqlConnection(QuizHelper.ConnStr))
                {
                    con.Open();
                    using (var tx = con.BeginTransaction())
                    {
                        try
                        {
                            foreach (RepeaterItem item in rptAnswers.Items)
                            {
                                var hfAnswer = (HiddenField)item.FindControl("hfAnswerId");
                                var txtAwarded = (TextBox)item.FindControl("txtAwarded");
                                var txtFeedback = (TextBox)item.FindControl("txtFeedback");

                                decimal awarded;
                                if (!decimal.TryParse(txtAwarded.Text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out awarded))
                                {
                                    throw new InvalidOperationException("Marks must be numbers.");
                                }

                                string feedback = txtFeedback.Text.Trim();

                                // The WHERE clause also checks 0 <= marks <= the question's maximum.
                                using (var cmd = new SqlCommand(@"
                                    UPDATE a
                                    SET a.marksAwarded = @marks, a.adminFeedback = @feedback
                                    FROM dbo.QuizAnswerTable a
                                    INNER JOIN dbo.QuizQuestionTable q ON q.questionID = a.questionID
                                    INNER JOIN dbo.QuizAttemptTable t ON t.attemptID = a.attemptID
                                    WHERE a.answerID = @id AND a.attemptID = @attempt AND t.moduleID = @module
                                      AND q.questionType = 'Long' AND @marks >= 0 AND @marks <= q.marks", con, tx))
                                {
                                    cmd.Parameters.Add(new SqlParameter("@marks", SqlDbType.Decimal) { Precision = 5, Scale = 2, Value = awarded });
                                    cmd.Parameters.Add("@feedback", SqlDbType.NVarChar, 500).Value =
                                        feedback.Length == 0 ? (object)DBNull.Value : feedback;
                                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = int.Parse(hfAnswer.Value);
                                    cmd.Parameters.Add("@attempt", SqlDbType.Int).Value = AttemptId;
                                    cmd.Parameters.Add("@module", SqlDbType.Int).Value = ModuleId;

                                    if (cmd.ExecuteNonQuery() == 0)
                                    {
                                        throw new InvalidOperationException("Marks must be between 0 and the maximum.");
                                    }
                                }
                            }

                            // Every long answer now has marks, so the attempt can be graded.
                            QuizHelper.RefreshAttempt(con, tx, AttemptId);
                            tx.Commit();
                        }
                        catch
                        {
                            try { tx.Rollback(); } catch { }
                            throw;
                        }
                    }
                }

                ShowMessage("Marks saved. The student can now see the result.", true);
                LoadAttempts();
                LoadAnswers();
            }
            catch (InvalidOperationException ex)
            {
                ShowMessage(ex.Message, false);
            }
            catch (Exception)
            {
                ShowMessage("Unable to save the marks. Please try again.", false);
            }
        }

        private void ShowMessage(string message, bool success)
        {
            lblMessage.Text = HttpUtility.HtmlEncode(message);
            lblMessage.CssClass = success ? "qz-msg qz-msg-ok" : "qz-msg qz-msg-err";
            lblMessage.Visible = true;
        }
    }
}