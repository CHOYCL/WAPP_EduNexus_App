
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
                return ConfigurationManager
                    .ConnectionStrings["ConnectionString"]
                    .ConnectionString;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Check the logged-in user and load their greeting.
            // Do this on every request so the greeting stays correct.
            int userId;
            bool isValidUser = LoadUserGreeting(out userId);

            if (!IsPostBack)
            {
                // Keep the login message hidden.
                ltLearningMessage.Text = string.Empty;

                // No valid logged-in account: show the guest view.
                if (!isValidUser)
                {
                    ShowEmptyLearningPage();
                    return;
                }

                try
                {
                    // Load summary counts.
                    int enrolledCount = LoadSummaryCounts(userId);

                    // Load courses currently in progress.
                    LoadInProgressCourses(userId, enrolledCount);

                    // Load certificates belonging to this user.
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

        // Display the username only if the account exists in Users.
        private bool LoadUserGreeting(out int userId)
        {
            userId = 0;
            ltUsername.Text = string.Empty;

            // Read the session established by the login page.
            if (!int.TryParse(
                Convert.ToString(Session["UserId"]),
                out userId))
            {
                return false;
            }

            try
            {
                using (SqlConnection con =
                    new SqlConnection(ConnectionString))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT Username
                      FROM dbo.Users
                      WHERE Id = @UserId", con))
                {
                    cmd.Parameters.Add(
                        "@UserId", SqlDbType.Int).Value = userId;

                    con.Open();

                    object result = cmd.ExecuteScalar();

                    // Hide the greeting if the account does not exist.
                    if (result == null ||
                        result == DBNull.Value ||
                        string.IsNullOrWhiteSpace(
                            Convert.ToString(result)))
                    {
                        userId = 0;
                        return false;
                    }

                    // Encode the username before displaying it.
                    ltUsername.Text = HttpUtility.HtmlEncode(
                        Convert.ToString(result));

                    return true;
                }
            }
            catch
            {
                // If the account cannot be verified, hide the greeting.

                ltUsername.Text = string.Empty;
                userId = 0;

                return false;
            }
        }

        // Default display when nobody is logged in.
        private void ShowEmptyLearningPage()
        {
            ltEnrolledCount.Text = "0";
            ltInProgressCount.Text = "0";
            ltCertificateCount.Text = "0";

            // Do not display a login-required message.
            ltLearningMessage.Text = string.Empty;

            rptInProgressCourses.Visible = false;
            rptCertificates.Visible = false;

            // Show Browse courses for guests.
            pnlNoProgress.Visible = true;

            ltNoProgressMessage.Text =
                "You haven't enrolled in any courses yet.";

            lnkBrowseCourses.Visible = true;

            // Display the empty certificate state.
            pnlNoCertificates.Visible = true;
        }

        // Load enrollment and certificate summary counts.
        private int LoadSummaryCounts(int userId)
        {
            int enrolledCount = 0;

            using (SqlConnection con =
                new SqlConnection(ConnectionString))
            {
                con.Open();

                string enrollmentQuery = @"
                    SELECT
                        COUNT(*) AS EnrolledCount,
                        COALESCE(
                            SUM(
                                CASE
                                    WHEN LTRIM(RTRIM([Status]))
                                         = N'In Progress'
                                    THEN 1
                                    ELSE 0
                                END
                            ), 0
                        ) AS InProgressCount
                    FROM dbo.Enrollments
                    WHERE UserID = @UserID;";

                using (SqlCommand cmd =
                    new SqlCommand(enrollmentQuery, con))
                {
                    cmd.Parameters.Add(
                        "@UserID", SqlDbType.Int).Value = userId;

                    using (SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            enrolledCount = Convert.ToInt32(
                                reader["EnrolledCount"]);

                            ltEnrolledCount.Text =
                                enrolledCount.ToString();

                            ltInProgressCount.Text =
                                Convert.ToString(
                                    reader["InProgressCount"]);
                        }
                    }
                }

                string certificateQuery = @"
                    SELECT COUNT(*)
                    FROM dbo.Certificates
                    WHERE UserID = @UserID;";

                using (SqlCommand cmd =
                    new SqlCommand(certificateQuery, con))
                {
                    cmd.Parameters.Add(
                        "@UserID", SqlDbType.Int).Value = userId;

                    ltCertificateCount.Text =
                        Convert.ToString(cmd.ExecuteScalar());
                }
            }

            return enrolledCount;
        }

        // Display only courses whose status is In Progress.
        private void LoadInProgressCourses(
            int userId, int enrolledCount)
        {
            DataTable dt = new DataTable();

            string query = @"
                SELECT
                    e.courseID,
                    c.courseTitle,
                    e.EnrollmentDate
                FROM dbo.Enrollments AS e
                INNER JOIN dbo.CourseDetailTable AS c
                    ON c.courseID = e.courseID
                WHERE e.UserID = @UserID
                  AND LTRIM(RTRIM(e.[Status])) = N'In Progress'
                ORDER BY e.EnrollmentDate DESC;";

            using (SqlConnection con =
                new SqlConnection(ConnectionString))
            using (SqlCommand cmd =
                new SqlCommand(query, con))
            using (SqlDataAdapter da =
                new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add(
                    "@UserID", SqlDbType.Int).Value = userId;

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
        }

        // Load certificates belonging to the logged-in user.
        private void LoadCertificates(int userId)
        {
            DataTable dt = new DataTable();

            string query = @"
                SELECT
                    cert.CertificateID,
                    cert.CertificateNumber,
                    cert.DateCreated,
                    c.courseID,
                    c.courseTitle,
                    c.Category
                FROM dbo.Certificates AS cert
                LEFT JOIN dbo.CourseDetailTable AS c
                    ON c.courseID = cert.courseID
                WHERE cert.UserID = @UserID
                ORDER BY cert.DateCreated DESC;";

            using (SqlConnection con =
                new SqlConnection(ConnectionString))
            using (SqlCommand cmd =
                new SqlCommand(query, con))
            using (SqlDataAdapter da =
                new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add(
                    "@UserID", SqlDbType.Int).Value = userId;

                da.Fill(dt);
            }

            rptCertificates.DataSource = dt;
            rptCertificates.DataBind();

            pnlNoCertificates.Visible = dt.Rows.Count == 0;
        }
    }
}
