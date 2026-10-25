using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP_EduNexus_App.Admin
{
    public partial class AdminManageDiscussion : Page
    {
        private string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString; }
        }

        private int CurrentUserId
        {
            get
            {
                int userId;
                return int.TryParse(Convert.ToString(Session["UserId"]), out userId) ? userId : 0;
            }
        }

        protected string FormatMalaysiaTime(object value, string format)
        {
            if (value == null || value == DBNull.Value)
            {
                return string.Empty;
            }

            DateTime storedUtc = DateTime.SpecifyKind(Convert.ToDateTime(value), DateTimeKind.Utc);
            TimeZoneInfo malaysiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(storedUtc, malaysiaTimeZone).ToString(format);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Username"] == null ||
                !string.Equals(Convert.ToString(Session["Role"]), "Admin", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/CommonPage/LoginPage.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadCourses();
            }

            LoadDiscussions();

            int discussionId;
            if (int.TryParse(Request.QueryString["discussion"], out discussionId))
            {
                LoadSelectedDiscussion(discussionId);
            }
        }

        private void LoadCourses()
        {
            adminCourseFilter.Items.Clear();
            adminCourseFilter.Items.Add(new ListItem("All courses", "all"));

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                using (SqlCommand command = new SqlCommand(
                    "SELECT courseID, courseTitle FROM dbo.CourseDetailTable ORDER BY courseTitle;",
                    connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            adminCourseFilter.Items.Add(new ListItem(
                                Convert.ToString(reader["courseTitle"]),
                                Convert.ToString(reader["courseID"])));
                        }
                    }
                }
            }
            catch (SqlException)
            {
                SetNotice("I couldn't load the course filters. Check the database connection and try again.");
            }
        }

        private void LoadDiscussions()
        {
            const string query = @"
                SELECT discussion.DiscussionID, discussion.courseID AS CourseID,
                       discussion.UserID, discussion.Title, discussion.Details,
                       discussion.Status, discussion.CreatedAt,
                       course.courseTitle AS CourseTitle, appUser.Username,
                       COUNT(reply.ReplyID) AS ReplyCount
                FROM dbo.DiscussionTable AS discussion
                INNER JOIN dbo.CourseDetailTable AS course ON course.courseID = discussion.courseID
                INNER JOIN dbo.Users AS appUser ON appUser.Id = discussion.UserID
                LEFT JOIN dbo.DiscussionReplyTable AS reply ON reply.DiscussionID = discussion.DiscussionID
                GROUP BY discussion.DiscussionID, discussion.courseID, discussion.UserID,
                         discussion.Title, discussion.Details, discussion.Status,
                         discussion.CreatedAt, course.courseTitle, appUser.Username
                ORDER BY discussion.CreatedAt DESC, discussion.DiscussionID DESC;";

            try
            {
                DataTable discussions = new DataTable();
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                {
                    adapter.Fill(discussions);
                }

                rptAdminDiscussionRows.DataSource = discussions;
                rptAdminDiscussionRows.DataBind();
            }
            catch (SqlException exception)
            {
                SetNotice(exception.Number == 208
                    ? "Discussion tables are missing. Run Database/07_Discussions.sql against Courses.mdf, then refresh Server Explorer."
                    : "I couldn't load discussions from the database. Please try again later.");
            }
        }

        private void LoadSelectedDiscussion(int discussionId)
        {
            const string discussionQuery = @"
                SELECT discussion.Title, discussion.Details, discussion.Status,
                       course.courseTitle AS CourseTitle, appUser.Username
                FROM dbo.DiscussionTable AS discussion
                INNER JOIN dbo.CourseDetailTable AS course ON course.courseID = discussion.courseID
                INNER JOIN dbo.Users AS appUser ON appUser.Id = discussion.UserID
                WHERE discussion.DiscussionID = @DiscussionID;";

            const string repliesQuery = @"
                SELECT reply.ReplyText, reply.CreatedAt, appUser.Username, appUser.Role
                FROM dbo.DiscussionReplyTable AS reply
                INNER JOIN dbo.Users AS appUser ON appUser.Id = reply.UserID
                WHERE reply.DiscussionID = @DiscussionID
                ORDER BY reply.CreatedAt, reply.ReplyID;";

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                using (SqlCommand command = new SqlCommand(discussionQuery, connection))
                {
                    command.Parameters.Add("@DiscussionID", SqlDbType.Int).Value = discussionId;
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            SetNotice("That discussion is no longer available.");
                            return;
                        }

                        ltAdminDiscussionTitle.Text = HttpUtility.HtmlEncode(Convert.ToString(reader["Title"]));
                        ltAdminDiscussionDetails.Text = HttpUtility.HtmlEncode(Convert.ToString(reader["Details"]));
                        ltAdminDiscussionMember.Text = HttpUtility.HtmlEncode(Convert.ToString(reader["Username"]));
                        ltAdminDiscussionCourse.Text = HttpUtility.HtmlEncode(Convert.ToString(reader["CourseTitle"]));
                        string status = Convert.ToString(reader["Status"]);
                        ltAdminDiscussionStatus.Text = HttpUtility.HtmlEncode(status);
                        adminStatusBadge.Attributes["class"] = "admin-status status-" + status.ToLowerInvariant();
                    }

                    DataTable replies = new DataTable();
                    using (SqlCommand replyCommand = new SqlCommand(repliesQuery, connection))
                    using (SqlDataAdapter replyAdapter = new SqlDataAdapter(replyCommand))
                    {
                        replyCommand.Parameters.Add("@DiscussionID", SqlDbType.Int).Value = discussionId;
                        replyAdapter.Fill(replies);
                    }

                    rptAdminReplies.DataSource = replies;
                    rptAdminReplies.DataBind();
                    adminEmptyReplies.Visible = replies.Rows.Count == 0;
                    adminDiscussionModal.Visible = true;
                }
            }
            catch (SqlException)
            {
                SetNotice("I couldn't load this discussion's replies. Please try again later.");
            }
        }

        protected void rptAdminDiscussionRows_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int discussionId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out discussionId))
            {
                return;
            }

            if (string.Equals(e.CommandName, "Open", StringComparison.OrdinalIgnoreCase))
            {
                RedirectToDiscussion(discussionId);
                return;
            }

            if (string.Equals(e.CommandName, "Delete", StringComparison.OrdinalIgnoreCase))
            {
                const string deleteQuery = "DELETE FROM dbo.DiscussionTable WHERE DiscussionID = @DiscussionID;";
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                {
                    command.Parameters.Add("@DiscussionID", SqlDbType.Int).Value = discussionId;
                    connection.Open();
                    command.ExecuteNonQuery();
                }

                SetNotice("The discussion and its replies were deleted.");
                LoadDiscussions();
            }
        }

        protected void saveAdminReply_Click(object sender, EventArgs e)
        {
            int discussionId;
            string replyText = adminReplyInput.Text.Trim();
            if (CurrentUserId <= 0 ||
                !int.TryParse(Request.QueryString["discussion"], out discussionId) ||
                replyText.Length == 0 || replyText.Length > 2000)
            {
                SetNotice("Write a reply before posting it.");
                return;
            }

            const string insertQuery = @"
                INSERT INTO dbo.DiscussionReplyTable (DiscussionID, UserID, ReplyText)
                SELECT @DiscussionID, @UserID, @ReplyText
                WHERE EXISTS (SELECT 1 FROM dbo.DiscussionTable WHERE DiscussionID = @DiscussionID);";

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.Add("@DiscussionID", SqlDbType.Int).Value = discussionId;
                    command.Parameters.Add("@UserID", SqlDbType.Int).Value = CurrentUserId;
                    command.Parameters.Add("@ReplyText", SqlDbType.NVarChar, 2000).Value = replyText;
                    connection.Open();
                    if (command.ExecuteNonQuery() == 0)
                    {
                        SetNotice("That discussion is no longer available.");
                        return;
                    }
                }

                RedirectToDiscussion(discussionId);
            }
            catch (SqlException)
            {
                SetNotice("I couldn't save the reply. Please check the database connection and try again.");
            }
        }

        protected string EncodeAttribute(object value)
        {
            return HttpUtility.HtmlAttributeEncode(Convert.ToString(value)).Replace("'", "&#39;");
        }

        protected string ReplyRole(object role)
        {
            return string.Equals(Convert.ToString(role), "Admin", StringComparison.OrdinalIgnoreCase)
                ? "Admin"
                : "Member";
        }

        private void RedirectToDiscussion(int discussionId)
        {
            Response.Redirect(ResolveUrl("~/Admin/AdminManageDiscussion.aspx?discussion=" + discussionId), false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private void SetNotice(string message)
        {
            ltAdminNotice.Text = "<p class=\"admin-discussion-notice\">" +
                HttpUtility.HtmlEncode(message) + "</p>";
        }
    }
}
