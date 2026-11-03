using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI.WebControls;

namespace WAPP_EduNexus_App
{
    public partial class TakeQuiz : System.Web.UI.Page
    {
        private int _userId;
        private int _moduleId;
        private int _courseId;

        protected void Page_Load(object sender, EventArgs e)
        {
            _userId = QuizHelper.RequireUser(this);
            int.TryParse(Request.QueryString["moduleId"], out _moduleId);

            if (_moduleId == 0)
            {
                Block("Quiz not found.");
                return;
            }

            try
            {
                using (var con = new SqlConnection(QuizHelper.ConnStr))
                {
                    con.Open();

                    // Module details
                    string title;
                    int passPercentage;
                    using (var cmd = new SqlCommand(
                        "SELECT moduleTitle, courseID, passPercentage FROM dbo.ModuleTable WHERE moduleID = @m", con))
                    {
                        cmd.Parameters.Add("@m", SqlDbType.Int).Value = _moduleId;
                        using (var r = cmd.ExecuteReader())
                        {
                            if (!r.Read())
                            {
                                Block("Quiz not found.");
                                return;
                            }
                            title = r.GetString(0);
                            _courseId = r.GetInt32(1);
                            passPercentage = r.GetInt32(2);
                        }
                    }

                    ltModuleTitle.Text = HttpUtility.HtmlEncode(title);
                    lnkBack.NavigateUrl = "ModuleLearn.aspx?courseId=" + _courseId + "&moduleId=" + _moduleId;

                    // Access rules (checked on every request, including the submit)
                    if (!QuizHelper.IsEnrolled(con, _userId, _courseId))
                    {
                        Block("You must be enrolled in this course to take the quiz.");
                        return;
                    }

                    AttemptState state = QuizHelper.GetAttemptState(con, _userId, _moduleId);
                    if (state.Passed)
                    {
                        Block("You have already passed this quiz.");
                        return;
                    }
                    if (state.Pending)
                    {
                        Block("Your latest attempt is waiting to be marked. You can try again once the result is ready.");
                        return;
                    }
                    if (state.Used >= QuizHelper.MaxAttempts)
                    {
                        Block("You have used all " + QuizHelper.MaxAttempts + " attempts for this quiz.");
                        return;
                    }

                    if (!IsPostBack)
                    {
                        // Only the question text and options are loaded here: never the answers.
                        var questions = new DataTable();
                        using (var cmd = new SqlCommand(@"
                            SELECT questionID, questionType, questionText, optionA, optionB, optionC, optionD, marks
                            FROM dbo.QuizQuestionTable
                            WHERE moduleID = @m
                            ORDER BY questionID", con))
                        using (var da = new SqlDataAdapter(cmd))
                        {
                            cmd.Parameters.Add("@m", SqlDbType.Int).Value = _moduleId;
                            da.Fill(questions);
                        }

                        if (questions.Rows.Count == 0)
                        {
                            Block("This quiz has no questions yet.");
                            return;
                        }

                        int totalMarks = 0;
                        foreach (DataRow row in questions.Rows) totalMarks += Convert.ToInt32(row["marks"]);

                        ltInfo.Text = "Attempt " + (state.Used + 1) + " of " + QuizHelper.MaxAttempts +
                                      " &ndash; " + totalMarks + " marks &ndash; pass mark " + passPercentage + "%";

                        rptQuestions.DataSource = questions;
                        rptQuestions.DataBind();
                    }
                }
            }
            catch (Exception)
            {
                Block("Unable to load the quiz. Please try again later.");
            }
        }

        // Show the right input for each question type.
        protected void rptQuestions_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

            var row = (DataRowView)e.Item.DataItem;
            string type = Convert.ToString(row["questionType"]);

            var rbl = (RadioButtonList)e.Item.FindControl("rblOptions");
            var txtShort = (TextBox)e.Item.FindControl("txtShort");
            var txtLong = (TextBox)e.Item.FindControl("txtLong");

            rbl.Visible = type == "MCQ";
            txtShort.Visible = type == "Short";
            txtLong.Visible = type == "Long";

            if (type == "MCQ")
            {
                foreach (string letter in new[] { "A", "B", "C", "D" })
                {
                    string text = Convert.ToString(row["option" + letter]);
                    if (text.Length > 0) rbl.Items.Add(new ListItem(text, letter));
                }
            }
        }

        // ---------------------------------------------------------------
        // Submit: save the attempt, mark what can be marked automatically
        // ---------------------------------------------------------------

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (!pnlQuiz.Visible) return;

            int attemptId = 0;

            try
            {
                using (var con = new SqlConnection(QuizHelper.ConnStr))
                {
                    con.Open();
                    using (var tx = con.BeginTransaction())
                    {
                        try
                        {
                            // Re-check the rules inside the transaction.
                            AttemptState state = QuizHelper.GetAttemptState(con, _userId, _moduleId, tx);
                            if (!state.CanAttempt)
                            {
                                tx.Rollback();
                                Block("You cannot start another attempt for this quiz.");
                                return;
                            }

                            // Answer key, loaded on the server only.
                            var key = new Dictionary<int, DataRow>();
                            var keyTable = new DataTable();
                            using (var cmd = new SqlCommand(@"
                                SELECT questionID, questionType, correctOption, modelAnswer, marks
                                FROM dbo.QuizQuestionTable
                                WHERE moduleID = @m", con, tx))
                            using (var da = new SqlDataAdapter(cmd))
                            {
                                cmd.Parameters.Add("@m", SqlDbType.Int).Value = _moduleId;
                                da.Fill(keyTable);
                            }
                            foreach (DataRow row in keyTable.Rows) key[Convert.ToInt32(row["questionID"])] = row;

                            // Questions the student actually saw on the page.
                            var shown = new List<RepeaterItem>();
                            int totalMarks = 0;
                            foreach (RepeaterItem item in rptQuestions.Items)
                            {
                                int questionId = int.Parse(((HiddenField)item.FindControl("hfQuestionId")).Value);
                                if (!key.ContainsKey(questionId)) continue;
                                shown.Add(item);
                                totalMarks += Convert.ToInt32(key[questionId]["marks"]);
                            }

                            // The attempt number is the next free one (the UNIQUE constraint blocks duplicates).
                            using (var cmd = new SqlCommand(@"
                                INSERT INTO dbo.QuizAttemptTable (UserID, moduleID, attemptNo, totalMarks)
                                VALUES (@u, @m, @n, @t);
                                SELECT CAST(SCOPE_IDENTITY() AS INT);", con, tx))
                            {
                                cmd.Parameters.Add("@u", SqlDbType.Int).Value = _userId;
                                cmd.Parameters.Add("@m", SqlDbType.Int).Value = _moduleId;
                                cmd.Parameters.Add("@n", SqlDbType.Int).Value = state.Used + 1;
                                cmd.Parameters.Add("@t", SqlDbType.Int).Value = totalMarks;
                                attemptId = (int)cmd.ExecuteScalar();
                            }

                            foreach (RepeaterItem item in shown)
                            {
                                int questionId = int.Parse(((HiddenField)item.FindControl("hfQuestionId")).Value);
                                DataRow q = key[questionId];
                                string type = Convert.ToString(q["questionType"]);
                                int marks = Convert.ToInt32(q["marks"]);

                                string selected = null;
                                string answerText = null;
                                decimal? awarded;

                                if (type == "MCQ")
                                {
                                    var rbl = (RadioButtonList)item.FindControl("rblOptions");
                                    selected = string.IsNullOrEmpty(rbl.SelectedValue) ? null : rbl.SelectedValue;
                                    awarded = selected == Convert.ToString(q["correctOption"]) ? marks : 0;
                                }
                                else if (type == "Short")
                                {
                                    answerText = ((TextBox)item.FindControl("txtShort")).Text.Trim();
                                    awarded = QuizHelper.ShortAnswerMatches(answerText, Convert.ToString(q["modelAnswer"])) ? marks : 0;
                                }
                                else
                                {
                                    // Long answer: the admin marks it. A blank answer scores 0 straight away.
                                    answerText = ((TextBox)item.FindControl("txtLong")).Text.Trim();
                                    if (answerText.Length > 4000) answerText = answerText.Substring(0, 4000);
                                    awarded = answerText.Length == 0 ? (decimal?)0m : null;
                                }

                                using (var cmd = new SqlCommand(@"
                                    INSERT INTO dbo.QuizAnswerTable (attemptID, questionID, selectedOption, answerText, marksAwarded)
                                    VALUES (@a, @q, @sel, @txt, @marks)", con, tx))
                                {
                                    cmd.Parameters.Add("@a", SqlDbType.Int).Value = attemptId;
                                    cmd.Parameters.Add("@q", SqlDbType.Int).Value = questionId;
                                    cmd.Parameters.Add("@sel", SqlDbType.Char, 1).Value = (object)selected ?? DBNull.Value;
                                    cmd.Parameters.Add("@txt", SqlDbType.NVarChar, -1).Value = (object)answerText ?? DBNull.Value;
                                    cmd.Parameters.Add(new SqlParameter("@marks", SqlDbType.Decimal)
                                    {
                                        Precision = 5,
                                        Scale = 2,
                                        Value = (object)awarded ?? DBNull.Value
                                    });
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            // Graded now, or Pending if any long answer is waiting for the admin.
                            QuizHelper.RefreshAttempt(con, tx, attemptId);
                            tx.Commit();
                        }
                        catch
                        {
                            try { tx.Rollback(); } catch { }
                            throw;
                        }
                    }
                }
            }
            catch (Exception)
            {
                Block("Your answers could not be saved. Please try again.");
                return;
            }

            Response.Redirect("QuizResult.aspx?attemptId=" + attemptId, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private void Block(string message)
        {
            lblMessage.Text = HttpUtility.HtmlEncode(message);
            lblMessage.CssClass = "qz-msg qz-msg-err";
            lblMessage.Visible = true;
            pnlQuiz.Visible = false;
        }
    }
}