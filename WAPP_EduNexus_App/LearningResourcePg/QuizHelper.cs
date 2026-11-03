using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web.UI;

namespace WAPP_EduNexus_App
{
    /// <summary>What one student has done so far on one module quiz.</summary>
    public class AttemptState
    {
        public int Used;
        public bool Passed;
        public bool Pending;
        public int LatestAttemptId;

        public int Left
        {
            get { return Math.Max(0, QuizHelper.MaxAttempts - Used); }
        }

        // A new attempt is allowed only when: not passed yet, nothing waiting to be marked, attempts remain.
        public bool CanAttempt
        {
            get { return !Passed && !Pending && Used < QuizHelper.MaxAttempts; }
        }
    }

    /// <summary>Shared helpers for the module and quiz pages.</summary>
    public static class QuizHelper
    {
        public const int MaxAttempts = 3;

        public static string ConnStr
        {
            get { return ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString; }
        }

        // ---------------------------------------------------------------
        // Access control (uses the Session keys set by LoginPage)
        // ---------------------------------------------------------------

        /// <summary>Sends anyone who is not a signed-in admin to the login page and stops the request.</summary>
        public static void RequireAdmin(Page page)
        {
            if (!string.Equals(Convert.ToString(page.Session["Role"]), "Admin", StringComparison.Ordinal))
            {
                page.Response.Redirect(page.ResolveUrl("~/CommonPage/LoginPage.aspx"), true);
            }
        }

        /// <summary>Returns the signed-in user's Id, or sends the visitor to the login page and stops the request.</summary>
        public static int RequireUser(Page page)
        {
            int id;
            if (!int.TryParse(Convert.ToString(page.Session["UserId"]), out id) || id <= 0)
            {
                page.Response.Redirect(page.ResolveUrl("~/CommonPage/LoginPage.aspx"), true);
            }
            return id;
        }

        public static bool IsEnrolled(SqlConnection con, int userId, int courseId)
        {
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM dbo.Enrollments WHERE UserID = @u AND courseID = @c", con))
            {
                cmd.Parameters.Add("@u", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@c", SqlDbType.Int).Value = courseId;
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        // ---------------------------------------------------------------
        // Attempt rules
        // ---------------------------------------------------------------

        public static AttemptState GetAttemptState(SqlConnection con, int userId, int moduleId, SqlTransaction tx = null)
        {
            var state = new AttemptState();
            using (var cmd = new SqlCommand(@"
                SELECT COUNT(*),
                       ISNULL(SUM(CASE WHEN isPassed = 1 THEN 1 ELSE 0 END), 0),
                       ISNULL(SUM(CASE WHEN status = 'Pending' THEN 1 ELSE 0 END), 0),
                       ISNULL(MAX(attemptID), 0)
                FROM dbo.QuizAttemptTable
                WHERE UserID = @u AND moduleID = @m", con, tx))
            {
                cmd.Parameters.Add("@u", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@m", SqlDbType.Int).Value = moduleId;
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        state.Used = r.GetInt32(0);
                        state.Passed = r.GetInt32(1) > 0;
                        state.Pending = r.GetInt32(2) > 0;
                        state.LatestAttemptId = r.GetInt32(3);
                    }
                }
            }
            return state;
        }

        // ---------------------------------------------------------------
        // Marking and grading
        // ---------------------------------------------------------------

        public static string GradeFor(decimal percentage)
        {
            if (percentage >= 80) return "A";
            if (percentage >= 70) return "B";
            if (percentage >= 60) return "C";
            if (percentage >= 50) return "D";
            return "F";
        }

        /// <summary>Trim, collapse spaces and lower-case so "  Polymorphism " matches "polymorphism".</summary>
        public static string Normalize(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            return Regex.Replace(text.Trim(), @"\s+", " ").ToLowerInvariant();
        }

        /// <summary>acceptedAnswers is a list separated by | (the admin can also use new lines).</summary>
        public static bool ShortAnswerMatches(string given, string acceptedAnswers)
        {
            string g = Normalize(given);
            if (g.Length == 0) return false;

            foreach (string accepted in (acceptedAnswers ?? string.Empty).Split(new[] { '|', '\r', '\n' }))
            {
                if (Normalize(accepted) == g) return true;
            }
            return false;
        }

        /// <summary>
        /// Recalculates one attempt from its saved answers.
        /// While any answer is still waiting for the admin (marksAwarded IS NULL) the attempt stays Pending.
        /// Once every answer has marks, it becomes Graded with percentage, grade and pass/fail.
        /// </summary>
        public static void RefreshAttempt(SqlConnection con, SqlTransaction tx, int attemptId)
        {
            decimal score;
            int waiting, totalMarks, passPercentage;

            using (var cmd = new SqlCommand(@"
                SELECT ISNULL(SUM(a.marksAwarded), 0),
                       ISNULL(SUM(CASE WHEN a.answerID IS NOT NULL AND a.marksAwarded IS NULL THEN 1 ELSE 0 END), 0),
                       t.totalMarks,
                       m.passPercentage
                FROM dbo.QuizAttemptTable t
                INNER JOIN dbo.ModuleTable m ON m.moduleID = t.moduleID
                LEFT JOIN dbo.QuizAnswerTable a ON a.attemptID = t.attemptID
                WHERE t.attemptID = @id
                GROUP BY t.totalMarks, m.passPercentage", con, tx))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = attemptId;
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read()) return;
                    score = r.GetDecimal(0);
                    waiting = r.GetInt32(1);
                    totalMarks = r.GetInt32(2);
                    passPercentage = r.GetInt32(3);
                }
            }

            string status = "Pending";
            decimal? percentage = null;
            string grade = null;
            bool? passed = null;

            if (waiting == 0)
            {
                status = "Graded";
                decimal pct = totalMarks > 0 ? Math.Round(score * 100m / totalMarks, 2) : 0m;
                percentage = pct;
                grade = GradeFor(pct);
                passed = pct >= passPercentage;
            }

            using (var upd = new SqlCommand(@"
                UPDATE dbo.QuizAttemptTable
                SET score = @score,
                    status = @status,
                    percentage = @pct,
                    grade = @grade,
                    isPassed = @passed,
                    gradedDate = CASE WHEN @status = 'Graded' THEN GETDATE() ELSE NULL END
                WHERE attemptID = @id", con, tx))
            {
                // Decimal parameters need an explicit precision and scale or SQL Server rounds them to whole numbers.
                upd.Parameters.Add(new SqlParameter("@score", SqlDbType.Decimal) { Precision = 6, Scale = 2, Value = score });
                upd.Parameters.Add("@status", SqlDbType.NVarChar, 10).Value = status;
                upd.Parameters.Add(new SqlParameter("@pct", SqlDbType.Decimal) { Precision = 5, Scale = 2, Value = (object)percentage ?? DBNull.Value });
                upd.Parameters.Add("@grade", SqlDbType.NVarChar, 2).Value = (object)grade ?? DBNull.Value;
                upd.Parameters.Add("@passed", SqlDbType.Bit).Value = (object)passed ?? DBNull.Value;
                upd.Parameters.Add("@id", SqlDbType.Int).Value = attemptId;
                upd.ExecuteNonQuery();
            }
        }

        // ---------------------------------------------------------------
        // Display helpers
        // ---------------------------------------------------------------

        /// <summary>Shows 3 instead of 3.00 and 2.5 instead of 2.50. Returns - for NULL.</summary>
        public static string Mark(object value)
        {
            if (value == null || value == DBNull.Value) return "-";
            return Convert.ToDecimal(value).ToString("0.##");
        }

        /// <summary>Returns - for NULL or empty values.</summary>
        public static string Dash(object value)
        {
            string text = (value == null || value == DBNull.Value) ? string.Empty : Convert.ToString(value);
            return text.Length == 0 ? "-" : text;
        }
    }
}

