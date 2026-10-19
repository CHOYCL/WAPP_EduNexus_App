
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP_EduNexus_App
{
    public partial class BrowseCourse : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadCategories();
                LoadCourses();
            }
        }

        protected void btnApply_Click(object sender, EventArgs e)
        {
            LoadCourses(
                txtSearch.Text.Trim(),
                ddlCategory.SelectedValue,
                ddlLevel.SelectedValue
            );
        }

        private string QuoteColumn(string name)
        {
            return "[" + name.Replace("]", "]]") + "]";
        }

        private HashSet<string> GetColumns(
            SqlConnection con, string tableName)
        {
            DataTable schema = new DataTable();

            using (SqlCommand cmd = new SqlCommand(
                "SELECT TOP 0 * FROM " + tableName, con))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                da.Fill(schema);
            }

            var columns = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            foreach (DataColumn column in schema.Columns)
                columns.Add(column.ColumnName);

            return columns;
        }

        private void LoadCategories()
        {
            string connectionString =
                ConfigurationManager.ConnectionStrings[
                    "ConnectionString"].ConnectionString;

            ddlCategory.Items.Clear();
            ddlCategory.Items.Add(
                new ListItem("All categories", ""));

            try
            {
                using (SqlConnection con =
                    new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT TOP 100 * FROM dbo.CourseCategory", con))
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    foreach (DataRow row in dt.Rows)
                    {
                        string categoryValue = "";
                        string categoryName = "";

                        if (dt.Columns.Count >= 2)
                        {
                            categoryValue =
                                Convert.ToString(row[0]);

                            categoryName =
                                Convert.ToString(row[1]);
                        }
                        else if (dt.Columns.Count == 1)
                        {
                            categoryValue =
                                Convert.ToString(row[0]);

                            categoryName = categoryValue;
                        }

                        if (!string.IsNullOrWhiteSpace(categoryName))
                        {
                            // Use the category name as the filter value.
                            ddlCategory.Items.Add(
                                new ListItem(
                                    categoryName,
                                    categoryName));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ltBrowseMessage.Text =
                    "<p class='error'>Unable to load categories: " +
                    HttpUtility.HtmlEncode(ex.Message) + "</p>";
            }
        }

        private void LoadCourses(
            string searchText = null,
            string category = null,
            string level = null)
        {
            string connectionString =
                ConfigurationManager.ConnectionStrings[
                    "ConnectionString"].ConnectionString;

            DataTable courses = new DataTable();

            try
            {
                using (SqlConnection con =
                    new SqlConnection(connectionString))
                {
                    con.Open();

                    var courseColumns =
                        GetColumns(con, "dbo.CourseDetailTable");

                    var categoryColumns =
                        GetColumns(con, "dbo.CourseCategory");

                    var materialColumns =
                        GetColumns(con, "dbo.MaterialTable");

                    bool hasCategory =
                        courseColumns.Contains("Category");

                    bool hasLevel =
                        courseColumns.Contains("level");

                    bool hasDescription =
                        courseColumns.Contains("Description");

                    bool hasInstructor =
                        courseColumns.Contains("Instructor");

                    bool hasMaterialCourseId =
                        materialColumns.Contains("courseID");

                    // Build a lookup for the category name.
                    string categoryApply = "";
                    string categoryExpression =
                        hasCategory
                            ? "CONVERT(NVARCHAR(255), cd.[Category])"
                            : "CAST('' AS NVARCHAR(255))";

                    if (hasCategory &&
                        categoryColumns.Count >= 2)
                    {
                        DataTable categorySchema = new DataTable();

                        using (SqlCommand cmd = new SqlCommand(
                            "SELECT TOP 0 * FROM dbo.CourseCategory", con))
                        using (SqlDataAdapter da =
                            new SqlDataAdapter(cmd))
                        {
                            da.Fill(categorySchema);
                        }

                        string idColumn =
                            QuoteColumn(categorySchema.Columns[0].ColumnName);

                        string nameColumn =
                            QuoteColumn(categorySchema.Columns[1].ColumnName);

                        string storedValue =
                            "LTRIM(RTRIM(CONVERT(NVARCHAR(255), cd.[Category])))";

                        string idMatch =
                            storedValue +
                            " = LTRIM(RTRIM(CONVERT(NVARCHAR(255), cc." +
                            idColumn + ")))";

                        string nameMatch =
                            storedValue +
                            " = LTRIM(RTRIM(CONVERT(NVARCHAR(255), cc." +
                            nameColumn + ")))";

                        categoryApply =
                            " OUTER APPLY (" +
                            "SELECT TOP 1 CONVERT(NVARCHAR(255), cc." +
                            nameColumn + ") AS CategoryName " +
                            "FROM dbo.CourseCategory AS cc " +
                            "WHERE (" + idMatch + ") OR (" + nameMatch + ") " +
                            "ORDER BY CASE WHEN (" + idMatch +
                            ") THEN 0 ELSE 1 END" +
                            ") AS cat";

                        categoryExpression =
                            "COALESCE(cat.CategoryName, " +
                            "CONVERT(NVARCHAR(255), cd.[Category]))";
                    }

                    // Load the latest material's description and media path.
                    string materialApply = "";
                    bool hasMaterialDescription =
                        materialColumns.Contains("materialDescription");

                    string materialPathColumn = null;

                    foreach (string candidate in new[]
                    {
                        "materialType", "MediaPath", "Image",
                        "Thumbnail_img", "VideoPath", "FilePath"
                    })
                    {
                        if (materialColumns.Contains(candidate))
                        {
                            materialPathColumn = candidate;
                            break;
                        }
                    }

                    var materialSelect = new List<string>();

                    if (hasMaterialDescription)
                    {
                        materialSelect.Add(
                            "mt.[materialDescription] AS MaterialDescription");
                    }

                    if (materialPathColumn != null)
                    {
                        materialSelect.Add(
                            "mt." + QuoteColumn(materialPathColumn) +
                            " AS MaterialPath");
                    }

                    if (hasMaterialCourseId &&
                        materialSelect.Count > 0)
                    {
                        string orderBy =
                            materialColumns.Contains("materialID")
                                ? " ORDER BY mt.[materialID] DESC"
                                : "";

                        materialApply =
                            " OUTER APPLY (SELECT TOP 1 " +
                            string.Join(", ", materialSelect) +
                            " FROM dbo.MaterialTable AS mt " +
                            "WHERE mt.[courseID] = cd.[courseID]" +
                            orderBy + ") AS latestMat";
                    }

                    // Description: prefer course description, then material.
                    var descriptionParts = new List<string>();

                    if (hasDescription)
                    {
                        descriptionParts.Add(
                            "NULLIF(LTRIM(RTRIM(CONVERT(NVARCHAR(MAX), " +
                            "cd.[Description]))), '')");
                    }

                    if (hasMaterialDescription &&
                        materialApply != "")
                    {
                        descriptionParts.Add(
                            "NULLIF(LTRIM(RTRIM(CONVERT(NVARCHAR(MAX), " +
                            "latestMat.MaterialDescription))), '')");
                    }

                    string descriptionExpression =
                        descriptionParts.Count > 0
                            ? "COALESCE(" +
                              string.Join(", ", descriptionParts) +
                              ", CAST('' AS NVARCHAR(MAX)))"
                            : "CAST('' AS NVARCHAR(MAX))";

                    // Image: use a course thumbnail/image first.
                    var imageParts = new List<string>();

                    foreach (string candidate in new[]
                    {
                        "Thumbnail_img", "Image", "Thumbnail"
                    })
                    {
                        if (courseColumns.Contains(candidate))
                        {
                            imageParts.Add(
                                "NULLIF(LTRIM(RTRIM(CONVERT(NVARCHAR(1000), " +
                                "cd." + QuoteColumn(candidate) + "))), '')");
                        }
                    }

                    if (materialPathColumn != null &&
                        materialApply != "")
                    {
                        imageParts.Add(
                            "NULLIF(LTRIM(RTRIM(CONVERT(NVARCHAR(1000), " +
                            "latestMat.MaterialPath))), '')");
                    }

                    string imageExpression =
                        imageParts.Count > 0
                            ? "COALESCE(" +
                              string.Join(", ", imageParts) +
                              ")"
                            : "CAST(NULL AS NVARCHAR(1000))";

                    string lessonCountExpression =
                        hasMaterialCourseId
                            ? "(SELECT COUNT(*) FROM dbo.MaterialTable AS lm " +
                              "WHERE lm.[courseID] = cd.[courseID])"
                            : "0";

                    var sql = new StringBuilder();

                    sql.Append(
                        "SELECT cd.[courseID], cd.[courseTitle], ");

                    sql.Append(categoryExpression + " AS [Category], ");

                    sql.Append(
                        hasLevel
                            ? "CONVERT(NVARCHAR(100), cd.[level]) AS [Level], "
                            : "CAST('' AS NVARCHAR(100)) AS [Level], ");

                    sql.Append(descriptionExpression + " AS [Description], ");

                    sql.Append(imageExpression + " AS [ImagePath], ");

                    sql.Append(
                        lessonCountExpression + " AS [LessonCount] ");

                    sql.Append(
                        "FROM dbo.CourseDetailTable AS cd ");

                    sql.Append(categoryApply + " ");
                    sql.Append(materialApply + " ");
                    sql.Append("WHERE 1 = 1 ");

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = con;

                        if (!string.IsNullOrWhiteSpace(searchText))
                        {
                            var conditions = new List<string>
                            {
                                "cd.[courseTitle] LIKE @search",
                                descriptionExpression + " LIKE @search",
                                categoryExpression + " LIKE @search"
                            };

                            if (hasInstructor)
                            {
                                conditions.Add(
                                    "cd.[Instructor] LIKE @search");
                            }

                            sql.Append(
                                "AND (" + string.Join(" OR ", conditions) +
                                ") ");

                            cmd.Parameters.AddWithValue(
                                "@search", "%" + searchText + "%");
                        }

                        if (!string.IsNullOrWhiteSpace(category) &&
                            hasCategory)
                        {
                            sql.Append(
                                "AND " + categoryExpression + " = @category ");

                            cmd.Parameters.AddWithValue(
                                "@category", category);
                        }

                        if (!string.IsNullOrWhiteSpace(level) && hasLevel)
                        {
                            sql.Append("AND cd.[level] = @level ");

                            cmd.Parameters.AddWithValue(
                                "@level", level);
                        }

                        sql.Append("ORDER BY cd.[courseID] DESC");

                        cmd.CommandText = sql.ToString();

                        using (SqlDataAdapter da =
                            new SqlDataAdapter(cmd))
                        {
                            da.Fill(courses);
                        }
                    }
                }

                // Prepare an image URL for each card.
                if (!courses.Columns.Contains("ImageUrl"))
                {
                    courses.Columns.Add("ImageUrl", typeof(string));
                }

                foreach (DataRow row in courses.Rows)
                {
                    string imagePath =
                        Convert.ToString(row["ImagePath"]);

                    string extension =
                        Path.GetExtension(imagePath ?? "")
                            .ToLowerInvariant();

                    bool isImage =
                        new[] { ".jpg", ".jpeg", ".png", ".gif",
                                ".bmp", ".webp" }
                        .Contains(extension);

                    if (!isImage || string.IsNullOrWhiteSpace(imagePath))
                    {
                        row["ImageUrl"] =
                            ResolveUrl("~/IMG/PhotographyCardPhoto.jpg");
                    }
                    else if (imagePath.StartsWith("~/"))
                    {
                        row["ImageUrl"] = ResolveUrl(imagePath);
                    }
                    else if (imagePath.StartsWith("/"))
                    {
                        row["ImageUrl"] =
                            ResolveUrl("~" + imagePath);
                    }
                    else
                    {
                        row["ImageUrl"] =
                            ResolveUrl("~/" + imagePath.TrimStart('/'));
                    }
                }

                if (courses.Rows.Count == 0)
                {
                    rptCourses.Visible = false;
                    ltBrowseMessage.Text =
                        "<p class='no-courses'>No courses found.</p>";
                }
                else
                {
                    ltBrowseMessage.Text = "";
                    rptCourses.Visible = true;
                    rptCourses.DataSource = courses;
                    rptCourses.DataBind();
                }
            }
            catch (Exception ex)
            {
                rptCourses.Visible = false;

                ltBrowseMessage.Text =
                    "<p class='error'>Unable to load courses: " +
                    HttpUtility.HtmlEncode(ex.Message) + "</p>";
            }
        }
    }
}
