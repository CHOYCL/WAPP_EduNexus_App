using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace WAPP_EduNexus_App
{
    public partial class AdminManageQuiz : System.Web.UI.Page
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
                LoadQuestions();
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

        private void LoadQuestions()
        {
            var dt = new DataTable();
            using (var con = new SqlConnection(QuizHelper.ConnStr))
            using (var cmd = new SqlCommand(@"
                SELECT questionID, questionType, questionText, correctOption, modelAnswer, marks
                FROM dbo.QuizQuestionTable
                WHERE moduleID = @m
                ORDER BY questionID", con))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add("@m", SqlDbType.Int).Value = ModuleId;
                da.Fill(dt);
            }

            int totalMarks = 0;
            foreach (DataRow row in dt.Rows) totalMarks += Convert.ToInt32(row["marks"]);

            ltSummary.Text = dt.Rows.Count + " question(s), " + totalMarks + " marks in total";
            rptQuestions.Visible = dt.Rows.Count > 0;
            ltEmpty.Text = dt.Rows.Count == 0
                ? "<p class=\"qz-muted\">No questions yet. Add the first one above.</p>"
                : string.Empty;
            rptQuestions.DataSource = dt;
            rptQuestions.DataBind();
        }

        protected string TypeLabel(object type)
        {
            switch (Convert.ToString(type))
            {
                case "MCQ": return "Multiple choice";
                case "Short": return "Short answer";
                case "Long": return "Long answer";
                default: return Convert.ToString(type);
            }
        }

        /// <summary>Plain-text summary of how the question is marked.</summary>
        protected string AnswerKey(object item)
        {
            var row = (DataRowView)item;
            switch (Convert.ToString(row["questionType"]))
            {
                case "MCQ":
                    return "Correct: " + Convert.ToString(row["correctOption"]);
                case "Short":
                    var answers = Convert.ToString(row["modelAnswer"])
                        .Split(new[] { '|', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(a => a.Trim());
                    return "Accepts: " + string.Join(", ", answers);
                default:
                    return "Marked by hand";
            }
        }

        // ---------------------------------------------------------------
        // Validation (same rules as validateQuestionFields in the .aspx)
        // ---------------------------------------------------------------

        private string OptionText(string letter)
        {
            switch (letter)
            {
                case "A": return txtOptA.Text.Trim();
                case "B": return txtOptB.Text.Trim();
                case "C": return txtOptC.Text.Trim();
                default: return txtOptD.Text.Trim();
            }
        }

        private string ValidateQuestionFields()
        {
            string type = ddlType.SelectedValue;

            if (type == "MCQ")
            {
                if (OptionText("A").Length == 0 || OptionText("B").Length == 0)
                    return "Multiple choice needs at least options A and B.";
                if (OptionText(ddlCorrect.SelectedValue).Length == 0)
                    return "The correct option (" + ddlCorrect.SelectedValue + ") must have text.";
            }
            else if (type == "Short" && CleanAcceptedAnswers(txtShort.Text).Length == 0)
            {
                return "Enter at least one accepted answer.";
            }

            return null;
        }

        protected void cvQuestion_ServerValidate(object source, ServerValidateEventArgs args)
        {
            string error = ValidateQuestionFields();
            args.IsValid = (error == null);
            if (error != null) cvQuestion.ErrorMessage = error;
        }

        /// <summary>Turns "A | B" or one answer per line into "A|B".</summary>
        private static string CleanAcceptedAnswers(string text)
        {
            var parts = (text ?? string.Empty)
                .Split(new[] { '|', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => p.Length > 0);
            return string.Join("|", parts);
        }

        private static object DbOrNull(string text)
        {
            text = (text ?? string.Empty).Trim();
            return text.Length == 0 ? (object)DBNull.Value : text;
        }

        // ---------------------------------------------------------------
        // Create and update
        // ---------------------------------------------------------------

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            int marks, questionId;
            if (!int.TryParse(txtMarks.Text.Trim(), out marks) || marks < 1 || marks > 100)
            {
                ShowMessage("Marks must be a whole number from 1 to 100.", false);
                return;
            }

            bool editing = int.TryParse(hfQuestionId.Value, out questionId) && questionId > 0;
            string type = ddlType.SelectedValue;
            bool isMcq = type == "MCQ";

            // Keep only the fields that belong to this question type.
            object optionA = isMcq ? DbOrNull(txtOptA.Text) : DBNull.Value;
            object optionB = isMcq ? DbOrNull(txtOptB.Text) : DBNull.Value;
            object optionC = isMcq ? DbOrNull(txtOptC.Text) : DBNull.Value;
            object optionD = isMcq ? DbOrNull(txtOptD.Text) : DBNull.Value;
            object correct = isMcq ? (object)ddlCorrect.SelectedValue : DBNull.Value;

            object modelAnswer = DBNull.Value;
            if (type == "Short") modelAnswer = CleanAcceptedAnswers(txtShort.Text);
            else if (type == "Long") modelAnswer = DbOrNull(txtGuide.Text);

            string sql = editing
                ? @"UPDATE dbo.QuizQuestionTable
                    SET questionType = @type, questionText = @q, optionA = @a, optionB = @b, optionC = @c,
                        optionD = @d, correctOption = @correct, modelAnswer = @model, marks = @marks
                    WHERE questionID = @id AND moduleID = @m"
                : @"INSERT INTO dbo.QuizQuestionTable
                        (moduleID, questionType, questionText, optionA, optionB, optionC, optionD, correctOption, modelAnswer, marks)
                    VALUES (@m, @type, @q, @a, @b, @c, @d, @correct, @model, @marks)";

            try
            {
                using (var con = new SqlConnection(QuizHelper.ConnStr))
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.Add("@m", SqlDbType.Int).Value = ModuleId;
                    cmd.Parameters.Add("@type", SqlDbType.NVarChar, 10).Value = type;
                    cmd.Parameters.Add("@q", SqlDbType.NVarChar, 500).Value = txtQuestion.Text.Trim();
                    cmd.Parameters.Add("@a", SqlDbType.NVarChar, 250).Value = optionA;
                    cmd.Parameters.Add("@b", SqlDbType.NVarChar, 250).Value = optionB;
                    cmd.Parameters.Add("@c", SqlDbType.NVarChar, 250).Value = optionC;
                    cmd.Parameters.Add("@d", SqlDbType.NVarChar, 250).Value = optionD;
                    cmd.Parameters.Add("@correct", SqlDbType.Char, 1).Value = correct;
                    cmd.Parameters.Add("@model", SqlDbType.NVarChar, -1).Value = modelAnswer;
                    cmd.Parameters.Add("@marks", SqlDbType.Int).Value = marks;
                    if (editing) cmd.Parameters.Add("@id", SqlDbType.Int).Value = questionId;

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                ShowMessage(editing ? "Question updated." : "Question added.", true);
                ResetForm();
                LoadQuestions();
            }
            catch (Exception)
            {
                ShowMessage("Unable to save the question. Please try again.", false);
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ResetForm();
        }

        // ---------------------------------------------------------------
        // Edit and delete (Repeater buttons)
        // ---------------------------------------------------------------

        protected void rptQuestions_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int questionId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out questionId)) return;

            if (e.CommandName == "EditQuestion")
            {
                LoadQuestionIntoForm(questionId);
            }
            else if (e.CommandName == "DeleteQuestion")
            {
                DeleteQuestion(questionId);
            }
        }

        private void LoadQuestionIntoForm(int questionId)
        {
            var dt = new DataTable();
            using (var con = new SqlConnection(QuizHelper.ConnStr))
            using (var cmd = new SqlCommand(@"
                SELECT questionType, questionText, optionA, optionB, optionC, optionD, correctOption, modelAnswer, marks
                FROM dbo.QuizQuestionTable
                WHERE questionID = @id AND moduleID = @m", con))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = questionId;
                cmd.Parameters.Add("@m", SqlDbType.Int).Value = ModuleId;
                da.Fill(dt);
            }

            if (dt.Rows.Count == 0) return;
            DataRow r = dt.Rows[0];

            string type = Convert.ToString(r["questionType"]);
            string model = Convert.ToString(r["modelAnswer"]);
            string correct = Convert.ToString(r["correctOption"]);

            ddlType.SelectedValue = type;
            txtQuestion.Text = Convert.ToString(r["questionText"]);
            txtMarks.Text = Convert.ToString(r["marks"]);
            txtOptA.Text = Convert.ToString(r["optionA"]);
            txtOptB.Text = Convert.ToString(r["optionB"]);
            txtOptC.Text = Convert.ToString(r["optionC"]);
            txtOptD.Text = Convert.ToString(r["optionD"]);
            ddlCorrect.SelectedValue = correct.Length == 0 ? "A" : correct;
            txtShort.Text = type == "Short" ? model.Replace("|", Environment.NewLine) : string.Empty;
            txtGuide.Text = type == "Long" ? model : string.Empty;

            hfQuestionId.Value = questionId.ToString();
            ltFormTitle.Text = "Edit question";
            btnSave.Text = "Save changes";
            btnCancel.Visible = true;
        }

        private void DeleteQuestion(int questionId)
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
                            // Find the question's marks (also confirms it belongs to this module).
                            object marksValue;
                            using (var cmd = new SqlCommand(
                                "SELECT marks FROM dbo.QuizQuestionTable WHERE questionID = @q AND moduleID = @m", con, tx))
                            {
                                cmd.Parameters.Add("@q", SqlDbType.Int).Value = questionId;
                                cmd.Parameters.Add("@m", SqlDbType.Int).Value = ModuleId;
                                marksValue = cmd.ExecuteScalar();
                            }
                            if (marksValue == null)
                            {
                                tx.Rollback();
                                return;
                            }
                            int marks = Convert.ToInt32(marksValue);

                            // Which attempts answered this question? Their totals must be recalculated.
                            var attemptIds = new List<int>();
                            using (var cmd = new SqlCommand(
                                "SELECT attemptID FROM dbo.QuizAnswerTable WHERE questionID = @q", con, tx))
                            {
                                cmd.Parameters.Add("@q", SqlDbType.Int).Value = questionId;
                                using (var r = cmd.ExecuteReader())
                                {
                                    while (r.Read()) attemptIds.Add(r.GetInt32(0));
                                }
                            }

                            using (var cmd = new SqlCommand("DELETE FROM dbo.QuizAnswerTable WHERE questionID = @q", con, tx))
                            {
                                cmd.Parameters.Add("@q", SqlDbType.Int).Value = questionId;
                                cmd.ExecuteNonQuery();
                            }

                            using (var cmd = new SqlCommand("DELETE FROM dbo.QuizQuestionTable WHERE questionID = @q", con, tx))
                            {
                                cmd.Parameters.Add("@q", SqlDbType.Int).Value = questionId;
                                cmd.ExecuteNonQuery();
                            }

                            foreach (int attemptId in attemptIds)
                            {
                                using (var cmd = new SqlCommand(@"
                                    UPDATE dbo.QuizAttemptTable
                                    SET totalMarks = CASE WHEN totalMarks >= @marks THEN totalMarks - @marks ELSE 0 END
                                    WHERE attemptID = @a", con, tx))
                                {
                                    cmd.Parameters.Add("@marks", SqlDbType.Int).Value = marks;
                                    cmd.Parameters.Add("@a", SqlDbType.Int).Value = attemptId;
                                    cmd.ExecuteNonQuery();
                                }
                                QuizHelper.RefreshAttempt(con, tx, attemptId);
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

                if (hfQuestionId.Value == questionId.ToString()) ResetForm();
                ShowMessage("Question deleted.", true);
                LoadQuestions();
            }
            catch (Exception)
            {
                ShowMessage("Unable to delete the question. Please try again.", false);
            }
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private void ResetForm()
        {
            hfQuestionId.Value = string.Empty;
            ddlType.SelectedValue = "MCQ";
            txtQuestion.Text = string.Empty;
            txtMarks.Text = "1";
            txtOptA.Text = string.Empty;
            txtOptB.Text = string.Empty;
            txtOptC.Text = string.Empty;
            txtOptD.Text = string.Empty;
            ddlCorrect.SelectedIndex = 0;
            txtShort.Text = string.Empty;
            txtGuide.Text = string.Empty;
            ltFormTitle.Text = "Add a question";
            btnSave.Text = "Add question";
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
