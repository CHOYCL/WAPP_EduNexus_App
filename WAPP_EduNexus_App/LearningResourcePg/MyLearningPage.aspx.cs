using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;

namespace WAPP_EduNexus_App
{
    public partial class MyLearningPage : System.Web.UI.Page
    {
        private string ConnectionString
        {
            get
            {
                ConnectionStringSettings settings =
                    ConfigurationManager.ConnectionStrings["ConnectionString"];

                if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
                {
                    throw new InvalidOperationException(
                        "The ConnectionString connection string is missing from Web.config.");
                }

                return settings.ConnectionString;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            int userId;
            bool isValidUser = LoadUserGreeting(out userId);

            if (!IsPostBack)
            {
                ltLearningMessage.Text = string.Empty;

                if (!isValidUser)
                {
                    ShowEmptyLearningPage();
                    return;
                }

                try
                {
                    int enrolledCount = LoadSummaryCounts(userId);
                    LoadInProgressCourses(userId, enrolledCount);
                    LoadCertificates(userId);
                }
                catch (Exception)
                {
                    ltLearningMessage.Text =
                        "Unable to load learning information. Please try again later.";

                    pnlNoProgress.Visible = false;
                    pnlNoCertificates.Visible = false;
                    rptInProgressCourses.Visible = false;
                    rptCertificates.Visible = false;
                }
            }
        }

        // Verify the logged-in user against dbo.Users and display the greeting.
        private bool LoadUserGreeting(out int userId)
        {
            userId = 0;
            ltUsername.Text = string.Empty;

            if (!int.TryParse(Convert.ToString(Session["UserId"]), out userId))
            {
                return false;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT Username
                      FROM dbo.Users
                      WHERE Id = @UserId", con))
                {
                    cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    con.Open();

                    object result = cmd.ExecuteScalar();

                    if (result == null || result == DBNull.Value ||
                        string.IsNullOrWhiteSpace(Convert.ToString(result)))
                    {
                        userId = 0;
                        return false;
                    }

                    ltUsername.Text = HttpUtility.HtmlEncode(Convert.ToString(result));
                    return true;
                }
            }
            catch (Exception)
            {
                ltUsername.Text = string.Empty;
                userId = 0;
                return false;
            }
        }

        // Default display for visitors who are not logged in.
        private void ShowEmptyLearningPage()
        {
            ltEnrolledCount.Text = "0";
            ltInProgressCount.Text = "0";
            ltCertificateCount.Text = "0";
            ltLearningMessage.Text = string.Empty;

            rptInProgressCourses.DataSource = null;
            rptInProgressCourses.DataBind();
            rptCertificates.DataSource = null;
            rptCertificates.DataBind();

            rptInProgressCourses.Visible = false;
            rptCertificates.Visible = false;

            pnlNoProgress.Visible = true;
            ltNoProgressMessage.Text = "You haven't enrolled in any courses yet.";
            lnkBrowseCourses.Visible = true;

            pnlNoCertificates.Visible = true;
        }

        // Load the student's total enrolments, active courses and certificate count.
        // The certificate count reads dbo.CertificateTable (not dbo.Certificates).
        private int LoadSummaryCounts(int userId)
        {
            int enrolledCount = 0;

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();

                const string enrollmentQuery = @"
                    SELECT
                        COUNT(*) AS EnrolledCount,
                        COALESCE(SUM(
                            CASE
                                WHEN LTRIM(RTRIM(ISNULL([Status], N''))) = N'In Progress'
                                THEN 1
                                ELSE 0
                            END
                        ), 0) AS InProgressCount
                    FROM dbo.Enrollments
                    WHERE UserID = @UserID;";

                using (SqlCommand cmd = new SqlCommand(enrollmentQuery, con))
                {
                    cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            enrolledCount = Convert.ToInt32(reader["EnrolledCount"]);
                            ltEnrolledCount.Text = enrolledCount.ToString("N0");
                            ltInProgressCount.Text =
                                Convert.ToInt32(reader["InProgressCount"]).ToString("N0");
                        }
                    }
                }

                const string certificateQuery = @"
                    SELECT COUNT(*)
                    FROM dbo.CertificateTable
                    WHERE UserID = @UserID;";

                using (SqlCommand cmd = new SqlCommand(certificateQuery, con))
                {
                    cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                    int certificateCount = Convert.ToInt32(cmd.ExecuteScalar());
                    ltCertificateCount.Text = certificateCount.ToString("N0");
                }
            }

            return enrolledCount;
        }

        // Display only courses whose enrolment status is In Progress.
        private void LoadInProgressCourses(int userId, int enrolledCount)
        {
            DataTable dt = new DataTable();

            const string query = @"
                SELECT
                    e.courseID AS courseID,
                    c.courseTitle,
                    e.EnrollmentDate
                FROM dbo.Enrollments AS e
                INNER JOIN dbo.CourseDetailTable AS c
                    ON c.courseID = e.courseID
                WHERE e.UserID = @UserID
                  AND LTRIM(RTRIM(ISNULL(e.[Status], N''))) = N'In Progress'
                ORDER BY e.EnrollmentDate DESC;";

            using (SqlConnection con = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = new SqlCommand(query, con))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                da.Fill(dt);
            }

            rptInProgressCourses.DataSource = dt;
            rptInProgressCourses.DataBind();

            bool hasInProgressCourses = dt.Rows.Count > 0;
            rptInProgressCourses.Visible = hasInProgressCourses;
            pnlNoProgress.Visible = !hasInProgressCourses;

            if (!hasInProgressCourses)
            {
                if (enrolledCount == 0)
                {
                    ltNoProgressMessage.Text =
                        "You haven't enrolled in any courses yet.";
                    lnkBrowseCourses.Visible = true;
                }
                else
                {
                    ltNoProgressMessage.Text = "Nothing in progress";
                    lnkBrowseCourses.Visible = false;
                }
            }
            else
            {
                lnkBrowseCourses.Visible = false;
            }
        }

        // Load certificate records for this student from the table provided:
        // dbo.CertificateTable (CertificateID, UserID, CourseID,
        // CertificateNumber, DateCreated).
        private void LoadCertificates(int userId)
        {
            DataTable dt = new DataTable();

            const string query = @"
                SELECT
                    cert.CertificateID,
                    cert.CourseID AS CourseID,
                    cert.CertificateNumber,
                    cert.DateCreated,
                    course.courseTitle AS courseTitle,
                    course.Category AS Category
                FROM dbo.CertificateTable AS cert
                INNER JOIN dbo.CourseDetailTable AS course
                    ON course.courseID = cert.CourseID
                WHERE cert.UserID = @UserID
                ORDER BY cert.DateCreated DESC, cert.CertificateID DESC;";

            using (SqlConnection con = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = new SqlCommand(query, con))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                da.Fill(dt);
            }

            rptCertificates.DataSource = dt;
            rptCertificates.DataBind();

            bool hasCertificates = dt.Rows.Count > 0;
            rptCertificates.Visible = hasCertificates;
            pnlNoCertificates.Visible = !hasCertificates;
        }
    }
}
