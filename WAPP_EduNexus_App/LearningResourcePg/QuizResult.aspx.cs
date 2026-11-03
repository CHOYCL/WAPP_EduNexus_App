using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace WAPP_EduNexus_App
{
    public partial class QuizResult : System.Web.UI.Page
    {
        private int _reviewNumber;

        protected void Page_Load(object sender, EventArgs e)
        {
            int userId = QuizHelper.RequireUser(this);

            if (IsPostBack) return;

            int attemptId;
            int.TryParse(Request.QueryString["attemptId"], out attemptId);
            if (attemptId == 0)
            {
                Block("Result not found.");
                return;
            }

            try
            {
                using (var con = new SqlConnection(QuizHelper.ConnStr))
                {
                    con.Open();

                    // A student can only open their own attempts.
                    int attemptNo, totalMarks, moduleId, courseId, passPercentage;
                    decimal score;
                    decimal? percentage;
                    string status, grade, moduleTitle;
                    bool passed;

                    using (var cmd = new SqlCommand(@"
                        SELECT t.attemptNo, t.totalMarks, t.score, t.status, t.percentage, t.grade, t.isPassed,
                               t.moduleID, m.courseID, m.moduleTitle, m.passPercentage
                        FROM dbo.QuizAttemptTable t
                        INNER JOIN dbo.ModuleTable m ON m.moduleID = t.moduleID
                        WHERE t.attemptID = @a AND t.UserID = @u", con))
                    {
                        cmd.Parameters.Add("@a", SqlDbType.Int).Value = attemptId;
                        cmd.Parameters.Add("@u", SqlDbType.Int).Value = userId;
                        using (var r = cmd.ExecuteReader())
                        {
                            if (!r.Read())
                            {
                                Block("Result not found.");
                                return;
                            }
                            attemptNo = r.GetInt32(0);
                            totalMarks = r.GetInt32(1);
                            score = r.GetDecimal(2);
                            status = r.GetString(3);
                            percentage = r.IsDBNull(4) ? (decimal?)null : r.GetDecimal(4);
                            grade = r.IsDBNull(5) ? "-" : r.GetString(5);
                            passed = !r.IsDBNull(6) && r.GetBoolean(6);
                            moduleId = r.GetInt32(7);
                            courseId = r.GetInt32(8);
                            moduleTitle = r.GetString(9);
                            passPercentage = r.GetInt32(10);
                        }
                    }

                    lnkBack.NavigateUrl = "ModuleLearn.aspx?courseId=" + courseId + "&moduleId=" + moduleId;
                    AttemptState state = QuizHelper.GetAttemptState(con, userId, moduleId);

                    // ----- Summary -----
                    var html = new StringBuilder();
                    html.Append("<h2 class=\"qz-card-title\">" + HttpUtility.HtmlEncode(moduleTitle) +
                                " &ndash; attempt " + attemptNo + " of " + QuizHelper.MaxAttempts + "</h2>");

                    if (status == "Graded")
                    {
                        html.Append("<div class=\"qz-result\">");
                        html.Append(Stat(QuizHelper.Mark(score) + " / " + totalMarks, "Score"));
                        html.Append(Stat(QuizHelper.Mark(percentage) + "%", "Percentage"));
                        html.Append(Stat(grade, "Grade"));
                        html.Append("</div>");

                        if (passed)
                        {
                            html.Append("<p class=\"qz-pass\">You passed this quiz.</p>");
                        }
                        else
                        {
                            html.Append("<p class=\"qz-fail\">You did not reach the pass mark of " + passPercentage + "%.</p>");
                            html.Append(state.Left > 0
                                ? "<p class=\"qz-muted\">Attempts left: " + state.Left + " of " + QuizHelper.MaxAttempts + ".</p>"
                                : "<p class=\"qz-muted\">You have used all your attempts for this quiz.</p>");
                        }
                    }
                    else
                    {
                        html.Append("<p>Your answers are saved. Your long answers are waiting to be marked.</p>");
                        html.Append("<div class=\"qz-result\">");
                        html.Append(Stat(QuizHelper.Mark(score) + " / " + totalMarks, "Marked so far"));
                        html.Append("</div>");
                        html.Append("<p class=\"qz-muted\">Your percentage and grade appear here once marking is finished.</p>");
                    }

                    ltSummary.Text = html.ToString();

                    if (state.CanAttempt)
                    {
                        lnkRetake.Text = "Retake quiz (attempt " + (state.Used + 1) + " of " + QuizHelper.MaxAttempts + ")";
                        lnkRetake.NavigateUrl = "TakeQuiz.aspx?moduleId=" + moduleId;
                        lnkRetake.Visible = true;
                    }

                    // ----- Review -----
                    // Correct answers are only shown when the student passed or has no attempts left,
                    // so a failed attempt does not give away the answers for the next one.
                    bool reveal = status == "Graded" && (passed || state.Used >= QuizHelper.MaxAttempts);

                    var review = new DataTable();
                    using (var cmd = new SqlCommand(@"
                        SELECT q.questionType, q.questionText, q.marks, q.optionA, q.optionB, q.optionC, q.optionD,
                               a.selectedOption, a.answerText, a.marksAwarded, a.adminFeedback,
                               CASE WHEN @reveal = 1 THEN q.correctOption END AS correctOption,
                               CASE WHEN @reveal = 1 THEN q.modelAnswer END AS modelAnswer
                        FROM dbo.QuizAnswerTable a
                        INNER JOIN dbo.QuizQuestionTable q ON q.questionID = a.questionID
                        WHERE a.attemptID = @a
                        ORDER BY q.questionID", con))
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        cmd.Parameters.Add("@a", SqlDbType.Int).Value = attemptId;
                        cmd.Parameters.Add("@reveal", SqlDbType.Bit).Value = reveal;
                        da.Fill(review);
                    }

                    rptReview.DataSource = review;
                    rptReview.DataBind();
                }
            }
            catch (Exception)
            {
                Block("Unable to load the result. Please try again later.");
            }
        }

        // ---------------------------------------------------------------
        // Markup helpers
        // ---------------------------------------------------------------

        private static string Stat(string value, string label)
        {
            return "<div class=\"qz-stat\"><span class=\"qz-stat-num\">" + HttpUtility.HtmlEncode(value) +
                   "</span><span class=\"qz-stat-label\">" + HttpUtility.HtmlEncode(label) + "</span></div>";
        }

        private static string OptionLabel(DataRowView row, string letter)
        {
            if (string.IsNullOrEmpty(letter)) return "No answer";
            return letter + ". " + Convert.ToString(row["option" + letter]);
        }

        /// <summary>Builds one answer card (all text is encoded).</summary>
        protected string RenderReview(object item)
        {
            var row = (DataRowView)item;
            _reviewNumber++;

            string type = Convert.ToString(row["questionType"]);
            int maxMarks = Convert.ToInt32(row["marks"]);
            bool waiting = row["marksAwarded"] == DBNull.Value;
            decimal awarded = waiting ? 0m : Convert.ToDecimal(row["marksAwarded"]);

            string css = waiting ? "pending" : awarded >= maxMarks ? "ok" : awarded == 0 ? "bad" : "part";
            string yourAnswer = type == "MCQ"
                ? OptionLabel(row, Convert.ToString(row["selectedOption"]))
                : Convert.ToString(row["answerText"]);
            if (yourAnswer.Length == 0) yourAnswer = "No answer";

            var html = new StringBuilder();
            html.Append("<div class=\"qz-card qz-review qz-review-" + css + "\">");
            html.Append("<p class=\"qz-q-head\"><strong>" + _reviewNumber + ".</strong> " +
                        HttpUtility.HtmlEncode(Convert.ToString(row["questionText"])) +
                        " <span class=\"qz-muted\">" +
                        (waiting ? "Waiting for marking" : QuizHelper.Mark(awarded) + " / " + maxMarks) +
                        "</span></p>");
            html.Append("<div class=\"qz-student-answer\">" + HttpUtility.HtmlEncode(yourAnswer) + "</div>");

            // Only filled in when the correct answers may be shown.
            string correct = Convert.ToString(row["correctOption"]);
            string model = Convert.ToString(row["modelAnswer"]);

            if (type == "MCQ" && correct.Length > 0)
            {
                html.Append("<p class=\"qz-muted\">Correct answer: " + HttpUtility.HtmlEncode(OptionLabel(row, correct)) + "</p>");
            }
            else if (type == "Short" && model.Length > 0)
            {
                string accepted = string.Join(", ", model
                    .Split(new[] { '|', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(a => a.Trim()));
                html.Append("<p class=\"qz-muted\">Accepted answers: " + HttpUtility.HtmlEncode(accepted) + "</p>");
            }
            else if (type == "Long" && model.Length > 0)
            {
                html.Append("<p class=\"qz-muted\">Marking guide: " + HttpUtility.HtmlEncode(model) + "</p>");
            }

            string feedback = Convert.ToString(row["adminFeedback"]);
            if (feedback.Length > 0)
            {
                html.Append("<p><strong>Feedback:</strong> " + HttpUtility.HtmlEncode(feedback) + "</p>");
            }

            html.Append("</div>");
            return html.ToString();
        }

        private void Block(string message)
        {
            lblMessage.Text = HttpUtility.HtmlEncode(message);
            lblMessage.CssClass = "qz-msg qz-msg-err";
            lblMessage.Visible = true;
            pnlResult.Visible = false;
        }
    }
}