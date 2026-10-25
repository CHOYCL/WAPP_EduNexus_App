using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP_EduNexus_App
{
    public partial class MyDiscussions : Page
    {
        private string ConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            }
        }

        private int CurrentUserId
        {
            get
            {
                int userId;
                return int.TryParse(Convert.ToString(Session["UserId"]), out userId)
                    ? userId
                    : 0;
            }
        }

        protected int InitialThreadId { get; private set; }
        protected int InitialEditId { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Username"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/CommonPage/LoginPage.aspx");
                return;
            }

            if (string.Equals(Convert.ToString(Session["Role"]), "Admin", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Admin/AdminManageDiscussion.aspx");
                return;
            }

            int initialThreadId;
            InitialThreadId = int.TryParse(Request.QueryString["thread"], out initialThreadId)
                ? initialThreadId
                : 0;

            int initialEditId;
            InitialEditId = int.TryParse(Request.QueryString["edit"], out initialEditId)
                ? initialEditId
                : 0;

            if (!IsPostBack)
            {
                LoadCourses();
                if (InitialEditId > 0)
                {
                    LoadDiscussionForEdit(InitialEditId);
                }
            }

            LoadDiscussions();
        }

        protected string EncodeAttribute(object value)
        {
            return HttpUtility.HtmlAttributeEncode(Convert.ToString(value))
                .Replace("'", "&#39;");
        }

        protected string DiscussionOwner(object userId)
        {
            return Convert.ToInt32(userId) == CurrentUserId ? "mine" : "other";
        }

        protected bool CanMarkAnswered(object userId, object status)
        {
            return Convert.ToInt32(userId) == CurrentUserId &&
                string.Equals(Convert.ToString(status), "Open", StringComparison.OrdinalIgnoreCase);
        }

        protected bool IsDiscussionOwner(object userId)
        {
            return Convert.ToInt32(userId) == CurrentUserId;
        }

        protected string ReplyRole(object role)
        {
            return string.Equals(Convert.ToString(role), "Admin", StringComparison.OrdinalIgnoreCase)
                ? "Admin"
                : "Member";
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

        private void LoadCourses()
        {
            memberCourseFilter.Items.Clear();
            memberCourseFilter.Items.Add(new ListItem("All courses", "all"));

            memberDiscussionCourse.Items.Clear();
            memberDiscussionCourse.Items.Add(new ListItem("Choose a course", ""));

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
                            string courseId = Convert.ToString(reader["courseID"]);
                            string courseTitle = Convert.ToString(reader["courseTitle"]);

                            memberCourseFilter.Items.Add(new ListItem(courseTitle, courseId));
                            memberDiscussionCourse.Items.Add(new ListItem(courseTitle, courseId));
                        }
                    }
                }

                if (memberDiscussionCourse.Items.Count == 1)
                {
                    memberDiscussionCourse.Items[0].Text = "No courses available";
                    saveMemberDiscussion.Enabled = false;
                    SetNotice("There are no courses available to discuss yet.");
                }
            }
            catch (Exception)
            {
                memberDiscussionCourse.Items.Clear();
                memberDiscussionCourse.Items.Add(new ListItem("Courses could not be loaded", ""));
                saveMemberDiscussion.Enabled = false;
                SetNotice("I couldn't load the course list. Check the course database connection and try again.");
            }
        }

        private void LoadDiscussions()
        {
            const string discussionsQuery = @"
                SELECT
                    discussion.DiscussionID,
                    discussion.courseID AS CourseID,
                    discussion.UserID,
                    discussion.Title,
                    discussion.Details,
                    discussion.Status,
                    discussion.CreatedAt,
                    course.courseTitle AS CourseTitle,
                    appUser.Username,
                    COUNT(reply.ReplyID) AS ReplyCount
                FROM dbo.DiscussionTable AS discussion
                INNER JOIN dbo.CourseDetailTable AS course
                    ON course.courseID = discussion.courseID
                INNER JOIN dbo.Users AS appUser
                    ON appUser.Id = discussion.UserID
                LEFT JOIN dbo.DiscussionReplyTable AS reply
                    ON reply.DiscussionID = discussion.DiscussionID
                GROUP BY
                    discussion.DiscussionID, discussion.courseID, discussion.UserID,
                    discussion.Title, discussion.Details, discussion.Status,
                    discussion.CreatedAt, course.courseTitle, appUser.Username
                ORDER BY discussion.CreatedAt DESC, discussion.DiscussionID DESC;";

            const string repliesQuery = @"
                SELECT reply.ReplyID, reply.DiscussionID, reply.ReplyText, reply.CreatedAt,
                       appUser.Username, appUser.Role
                FROM dbo.DiscussionReplyTable AS reply
                INNER JOIN dbo.Users AS appUser ON appUser.Id = reply.UserID
                ORDER BY reply.CreatedAt, reply.ReplyID;";

            try
            {
                DataSet discussionData = new DataSet();
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (SqlDataAdapter discussionAdapter = new SqlDataAdapter(discussionsQuery, connection))
                    using (SqlDataAdapter replyAdapter = new SqlDataAdapter(repliesQuery, connection))
                    {
                        discussionAdapter.Fill(discussionData, "Discussions");
                        replyAdapter.Fill(discussionData, "Replies");
                    }
                }

                discussionData.Relations.Add(
                    "DiscussionReplies",
                    discussionData.Tables["Discussions"].Columns["DiscussionID"],
                    discussionData.Tables["Replies"].Columns["DiscussionID"],
                    false);

                rptDiscussionList.DataSource = discussionData.Tables["Discussions"];
                rptDiscussionList.DataBind();
            }
            catch (SqlException exception)
            {
                if (exception.Number == 208)
                {
                    SetNotice("Discussion tables are missing. Run Database/07_Discussions.sql against Courses.mdf, then reload this page.");
                }
                else
                {
                    SetNotice("I couldn't load discussions from the database. Please try again later.");
                }
            }
        }

        protected void rptDiscussionList_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
            {
                return;
            }

            DataRowView discussion = e.Item.DataItem as DataRowView;
            Repeater replies = e.Item.FindControl("rptThreadReplies") as Repeater;
            if (discussion != null && replies != null)
            {
                replies.DataSource = discussion.CreateChildView("DiscussionReplies");
                replies.DataBind();
            }
        }

        protected void rptDiscussionList_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int discussionId;
            if (CurrentUserId <= 0 || !int.TryParse(Convert.ToString(e.CommandArgument), out discussionId))
            {
                return;
            }

            if (string.Equals(e.CommandName, "Edit", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect(ResolveUrl("~/LearningResourcePg/MyDiscussions.aspx?edit=" + discussionId), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (string.Equals(e.CommandName, "Delete", StringComparison.OrdinalIgnoreCase))
            {
                const string deleteQuery = @"
                    DELETE FROM dbo.DiscussionTable
                     WHERE DiscussionID = @DiscussionID AND UserID = @UserID;";
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                {
                    command.Parameters.Add("@DiscussionID", SqlDbType.Int).Value = discussionId;
                    command.Parameters.Add("@UserID", SqlDbType.Int).Value = CurrentUserId;
                    connection.Open();
                    command.ExecuteNonQuery();
                }

                Response.Redirect(ResolveUrl("~/LearningResourcePg/MyDiscussions.aspx"), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!string.Equals(e.CommandName, "MarkAnswered", StringComparison.OrdinalIgnoreCase)) return;

            const string updateQuery = @"
                UPDATE dbo.DiscussionTable
                   SET Status = N'Answered'
                 WHERE DiscussionID = @DiscussionID
                   AND UserID = @UserID
                   AND Status = N'Open';";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(updateQuery, connection))
            {
                command.Parameters.Add("@DiscussionID", SqlDbType.Int).Value = discussionId;
                command.Parameters.Add("@UserID", SqlDbType.Int).Value = CurrentUserId;
                connection.Open();
                command.ExecuteNonQuery();
            }

            Response.Redirect(ResolveUrl("~/LearningResourcePg/MyDiscussions.aspx"), false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void saveMemberDiscussion_Click(object sender, EventArgs e)
        {
            string title = memberDiscussionTitle.Text.Trim();
            string details = memberDiscussionBody.Text.Trim();
            int courseId;
            int discussionId;
            bool isEditing = int.TryParse(memberDiscussionId.Value, out discussionId) && discussionId > 0;

            if (CurrentUserId <= 0)
            {
                Response.Redirect("~/CommonPage/LoginPage.aspx");
                return;
            }

            if (title.Length == 0 || title.Length > 120 ||
                details.Length == 0 || details.Length > 2000 ||
                !int.TryParse(memberDiscussionCourse.SelectedValue, out courseId))
            {
                SetNotice("Add a title and details, then choose one course for your discussion.");
                return;
            }

            string saveQuery = isEditing
                ? @"
                    UPDATE dbo.DiscussionTable
                       SET courseID = @CourseID, Title = @Title, Details = @Details
                     WHERE DiscussionID = @DiscussionID AND UserID = @UserID
                       AND EXISTS (SELECT 1 FROM dbo.CourseDetailTable WHERE courseID = @CourseID);"
                : @"
                    INSERT INTO dbo.DiscussionTable (courseID, UserID, Title, Details)
                    SELECT @CourseID, @UserID, @Title, @Details
                    WHERE EXISTS (SELECT 1 FROM dbo.CourseDetailTable WHERE courseID = @CourseID);";

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                using (SqlCommand command = new SqlCommand(saveQuery, connection))
                {
                    command.Parameters.Add("@CourseID", SqlDbType.Int).Value = courseId;
                    command.Parameters.Add("@UserID", SqlDbType.Int).Value = CurrentUserId;
                    command.Parameters.Add("@Title", SqlDbType.NVarChar, 120).Value = title;
                    command.Parameters.Add("@Details", SqlDbType.NVarChar, 2000).Value = details;
                    if (isEditing)
                    {
                        command.Parameters.Add("@DiscussionID", SqlDbType.Int).Value = discussionId;
                    }

                    connection.Open();
                    int rowsChanged = command.ExecuteNonQuery();
                    if (rowsChanged == 0)
                    {
                        if (isEditing)
                        {
                            InitialEditId = discussionId;
                            SetNotice("That discussion could not be updated. It may no longer be available.");
                        }
                        else
                        {
                            SetNotice("That course is no longer available. Choose a course from the list and try again.");
                        }
                        return;
                    }
                }

                Response.Redirect(ResolveUrl("~/LearningResourcePg/MyDiscussions.aspx"), false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (SqlException exception)
            {
                if (exception.Number == 208)
                {
                    SetNotice("Discussion tables are missing. Run Database/07_Discussions.sql against Courses.mdf, then try again.");
                }
                else
                {
                    SetNotice("I couldn't save your discussion. Please check the database connection and try again.");
                }
            }
        }

        private void LoadDiscussionForEdit(int discussionId)
        {
            const string query = @"
                SELECT courseID, Title, Details
                FROM dbo.DiscussionTable
                WHERE DiscussionID = @DiscussionID AND UserID = @UserID;";

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@DiscussionID", SqlDbType.Int).Value = discussionId;
                    command.Parameters.Add("@UserID", SqlDbType.Int).Value = CurrentUserId;
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            InitialEditId = 0;
                            SetNotice("You can edit only a discussion that you started.");
                            return;
                        }

                        memberDiscussionId.Value = Convert.ToString(discussionId);
                        memberDiscussionCourse.SelectedValue = Convert.ToString(reader["courseID"]);
                        memberDiscussionTitle.Text = Convert.ToString(reader["Title"]);
                        memberDiscussionBody.Text = Convert.ToString(reader["Details"]);
                        InitialEditId = discussionId;
                    }
                }
            }
            catch (SqlException)
            {
                SetNotice("I couldn't load this discussion for editing. Please try again later.");
            }
        }

        protected void saveThreadReply_Click(object sender, EventArgs e)
        {
            string replyText = threadReplyInput.Text.Trim();
            int discussionId;

            if (CurrentUserId <= 0)
            {
                Response.Redirect("~/CommonPage/LoginPage.aspx");
                return;
            }

            if (!int.TryParse(threadDiscussionId.Value, out discussionId) ||
                replyText.Length == 0 || replyText.Length > 2000)
            {
                int.TryParse(threadDiscussionId.Value, out discussionId);
                InitialThreadId = discussionId;
                SetNotice("Write a reply before posting it.");
                return;
            }

            const string insertReplyQuery = @"
                INSERT INTO dbo.DiscussionReplyTable (DiscussionID, UserID, ReplyText)
                SELECT @DiscussionID, @UserID, @ReplyText
                WHERE EXISTS (
                    SELECT 1 FROM dbo.DiscussionTable WHERE DiscussionID = @DiscussionID
                );";

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                using (SqlCommand command = new SqlCommand(insertReplyQuery, connection))
                {
                    command.Parameters.Add("@DiscussionID", SqlDbType.Int).Value = discussionId;
                    command.Parameters.Add("@UserID", SqlDbType.Int).Value = CurrentUserId;
                    command.Parameters.Add("@ReplyText", SqlDbType.NVarChar, 2000).Value = replyText;
                    connection.Open();

                    if (command.ExecuteNonQuery() == 0)
                    {
                        InitialThreadId = discussionId;
                        SetNotice("That discussion is no longer available.");
                        return;
                    }
                }

                Response.Redirect(ResolveUrl("~/LearningResourcePg/MyDiscussions.aspx?thread=" + discussionId), false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (SqlException)
            {
                InitialThreadId = discussionId;
                SetNotice("I couldn't save your reply. Please check the database connection and try again.");
            }
        }

        private void SetNotice(string message)
        {
            ltDiscussionNotice.Text = "<p class=\"discussion-notice\">" +
                HttpUtility.HtmlEncode(message) + "</p>";
        }
    }
}
