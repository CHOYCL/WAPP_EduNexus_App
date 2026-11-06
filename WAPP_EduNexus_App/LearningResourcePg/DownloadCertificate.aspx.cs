using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;

namespace WAPP_EduNexus_App
{
    public partial class DownloadCertificate : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Only the logged-in student who owns the certificate may download it.
            int userId;
            if (!int.TryParse(Convert.ToString(Session["UserId"]), out userId) || userId <= 0)
            {
                WriteError(401, "Please log in before downloading your certificate.");
                return;
            }

            int courseId;
            if (!int.TryParse(Request.QueryString["courseId"], out courseId) || courseId <= 0)
            {
                WriteError(400, "The course ID is missing or invalid.");
                return;
            }

            ConnectionStringSettings settings =
                ConfigurationManager.ConnectionStrings["ConnectionString"];

            if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
            {
                WriteError(500, "The database connection string 'ConnectionString' was not found in Web.config.");
                return;
            }

            string studentName = string.Empty;
            string courseTitle = string.Empty;
            string certificateNumber = string.Empty;
            DateTime dateCreated = DateTime.Today;
            int passedModules = 0;
            int totalModules = 0;
            bool certificateFound = false;

            // Read only a certificate that belongs to this logged-in user and
            // whose course enrollment has been marked Completed.
            const string query = @"
                SELECT TOP (1)
                    ct.CertificateNumber,
                    ct.DateCreated,
                    cd.courseTitle,
                    u.Username,
                    (
                        SELECT COUNT(*)
                        FROM dbo.ModuleTable AS m
                        WHERE m.courseID = ct.CourseID
                    ) AS TotalModules,
                    (
                        SELECT COUNT(*)
                        FROM dbo.ModuleTable AS m
                        WHERE m.courseID = ct.CourseID
                          AND EXISTS
                          (
                              SELECT 1
                              FROM dbo.QuizAttemptTable AS qa
                              WHERE qa.moduleID = m.moduleID
                                AND qa.UserID = ct.UserID
                                AND qa.isPassed = 1
                                AND LTRIM(RTRIM(ISNULL(qa.[status], N''))) = N'Graded'
                          )
                    ) AS PassedModules
                FROM dbo.CertificateTable AS ct
                INNER JOIN dbo.CourseDetailTable AS cd
                    ON cd.courseID = ct.CourseID
                INNER JOIN dbo.Users AS u
                    ON u.Id = ct.UserID
                WHERE ct.UserID = @UserID
                  AND ct.CourseID = @CourseID
                  AND EXISTS
                  (
                      SELECT 1
                      FROM dbo.Enrollments AS e
                      WHERE e.UserID = ct.UserID
                        AND e.courseID = ct.CourseID
                        AND LTRIM(RTRIM(ISNULL(e.[Status], N''))) = N'Completed'
                  )
                ORDER BY ct.DateCreated DESC, ct.CertificateID DESC;";

            try
            {
                using (SqlConnection con = new SqlConnection(settings.ConnectionString))
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                    cmd.Parameters.Add("@CourseID", SqlDbType.Int).Value = courseId;

                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            studentName = Convert.ToString(reader["Username"]);
                            courseTitle = Convert.ToString(reader["courseTitle"]);
                            certificateNumber = reader["CertificateNumber"] == DBNull.Value
                                ? string.Empty
                                : Convert.ToString(reader["CertificateNumber"]);

                            if (reader["DateCreated"] != DBNull.Value)
                            {
                                dateCreated = Convert.ToDateTime(reader["DateCreated"]);
                            }

                            passedModules = Convert.ToInt32(reader["PassedModules"]);
                            totalModules = Convert.ToInt32(reader["TotalModules"]);

                            certificateFound = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Do not expose database details to the browser.
                WriteError(500, "Unable to retrieve the certificate. Please try again later.");
                return;
            }

            if (!certificateFound)
            {
                WriteError(404, "No completed-course certificate was found for this account.");
                return;
            }

            byte[] pdfBytes;
            try
            {
                pdfBytes = CreateCertificatePdf(
                    studentName,
                    courseTitle,
                    certificateNumber,
                    dateCreated,
                    passedModules,
                    totalModules);
            }
            catch (Exception)
            {
                WriteError(500, "Unable to generate the PDF certificate.");
                return;
            }

            // Return the generated PDF as a file download.
            Response.Clear();
            Response.BufferOutput = true;
            Response.ContentType = "application/pdf";
            Response.Charset = string.Empty;
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.AddHeader(
                "Content-Disposition",
                "attachment; filename=\"EduNexus_Certificate_" + courseId.ToString(CultureInfo.InvariantCulture) + ".pdf\"");
            Response.AddHeader("Content-Length", pdfBytes.Length.ToString(CultureInfo.InvariantCulture));
            Response.BinaryWrite(pdfBytes);
            Response.Flush();
            Response.End();
        }

        private void WriteError(int statusCode, string message)
        {
            Response.Clear();
            Response.StatusCode = statusCode;
            Response.TrySkipIisCustomErrors = true;
            Response.ContentType = "text/plain";
            Response.ContentEncoding = Encoding.UTF8;
            Response.Write(message);
            Response.End();
        }

        // Creates a simple A4-landscape PDF without requiring a PDF NuGet package.
        private static byte[] CreateCertificatePdf(
            string studentName,
            string courseTitle,
            string certificateNumber,
            DateTime dateCreated,
            int passedModules,
            int totalModules)
        {
            const double pageWidth = 842.0;
            StringBuilder content = new StringBuilder();

            // Outer and inner certificate borders.
            content.AppendLine("q");
            content.AppendLine("0.08 0.16 0.29 RG 3 w 24 24 794 547 re S");
            content.AppendLine("0.78 0.59 0.25 RG 1.2 w 36 36 770 523 re S");
            content.AppendLine("0.78 0.59 0.25 RG 1 w 300 455 m 542 455 l S");
            content.AppendLine("0.78 0.59 0.25 RG 1 w 300 190 m 542 190 l S");
            content.AppendLine("Q");

            DrawCenteredText(content, "CERTIFICATE OF COMPLETION", "F2", 29, 492,
                "0.08 0.16 0.29", pageWidth);
            DrawCenteredText(content, "EDUNEXUS LEARNING PLATFORM", "F2", 12, 463,
                "0.55 0.39 0.16", pageWidth);
            DrawCenteredText(content, "This certificate is proudly presented to", "F1", 16, 405,
                "0.20 0.20 0.20", pageWidth);
            DrawCenteredText(content, studentName, "F2", 27, 350,
                "0.08 0.16 0.29", pageWidth);
            DrawCenteredText(content, "for successfully completing the course", "F1", 15, 305,
                "0.20 0.20 0.20", pageWidth);
            DrawCenteredText(content, courseTitle, "F2", 22, 250,
                "0.08 0.16 0.29", pageWidth);
            DrawCenteredText(content, "Modules Passed: " +
                passedModules.ToString(CultureInfo.InvariantCulture) + " / " +
                totalModules.ToString(CultureInfo.InvariantCulture),
                "F2", 14, 165, "0.08 0.16 0.29", pageWidth);
            DrawCenteredText(content, "Date of Completion: " + dateCreated.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture),
                "F1", 12, 137, "0.20 0.20 0.20", pageWidth);
            DrawCenteredText(content, "Certificate No.: " + certificateNumber,
                "F1", 10, 111, "0.35 0.35 0.35", pageWidth);
            DrawCenteredText(content, "EduNexus", "F2", 12, 70,
                "0.08 0.16 0.29", pageWidth);

            byte[] streamBytes = GetPdfTextEncoding().GetBytes(content.ToString());

            List<byte[]> objects = new List<byte[]>();
            objects.Add(Encoding.ASCII.GetBytes(
                "1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n"));
            objects.Add(Encoding.ASCII.GetBytes(
                "2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n"));
            objects.Add(Encoding.ASCII.GetBytes(
                "3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 842 595] " +
                "/Resources << /Font << /F1 4 0 R /F2 5 0 R >> >> /Contents 6 0 R >>\nendobj\n"));
            objects.Add(Encoding.ASCII.GetBytes(
                "4 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>\nendobj\n"));
            objects.Add(Encoding.ASCII.GetBytes(
                "5 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>\nendobj\n"));

            using (MemoryStream stream = new MemoryStream())
            {
                WriteAscii(stream, "6 0 obj\n<< /Length " + streamBytes.Length.ToString(CultureInfo.InvariantCulture) +
                    " >>\nstream\n");
                stream.Write(streamBytes, 0, streamBytes.Length);
                WriteAscii(stream, "\nendstream\nendobj\n");
                objects.Add(stream.ToArray());
            }

            using (MemoryStream output = new MemoryStream())
            {
                WriteAscii(output, "%PDF-1.4\n% EduNexus Certificate\n");

                long[] objectOffsets = new long[objects.Count + 1];
                for (int i = 0; i < objects.Count; i++)
                {
                    objectOffsets[i + 1] = output.Position;
                    byte[] objectBytes = objects[i];
                    output.Write(objectBytes, 0, objectBytes.Length);
                }

                long xrefOffset = output.Position;
                WriteAscii(output, "xref\n0 " + (objects.Count + 1).ToString(CultureInfo.InvariantCulture) + "\n");
                WriteAscii(output, "0000000000 65535 f \n");

                for (int i = 1; i < objectOffsets.Length; i++)
                {
                    WriteAscii(output,
                        objectOffsets[i].ToString("0000000000", CultureInfo.InvariantCulture) +
                        " 00000 n \n");
                }

                WriteAscii(output,
                    "trailer\n<< /Size " + (objects.Count + 1).ToString(CultureInfo.InvariantCulture) +
                    " /Root 1 0 R >>\nstartxref\n" +
                    xrefOffset.ToString(CultureInfo.InvariantCulture) + "\n%%EOF");

                return output.ToArray();
            }
        }

        private static void DrawCenteredText(
            StringBuilder content,
            string text,
            string fontName,
            int fontSize,
            double y,
            string rgbColor,
            double pageWidth)
        {
            string safeText = CleanPdfText(text);
            double estimatedTextWidth = safeText.Length * fontSize * 0.50;
            double x = (pageWidth - estimatedTextWidth) / 2.0;

            if (x < 55.0)
            {
                x = 55.0;
            }

            content.Append("BT /");
            content.Append(fontName);
            content.Append(" ");
            content.Append(fontSize.ToString(CultureInfo.InvariantCulture));
            content.Append(" Tf ");
            content.Append(rgbColor);
            content.Append(" rg 1 0 0 1 ");
            content.Append(x.ToString("0.##", CultureInfo.InvariantCulture));
            content.Append(" ");
            content.Append(y.ToString("0.##", CultureInfo.InvariantCulture));
            content.Append(" Tm (");
            content.Append(EscapePdfString(safeText));
            content.AppendLine(") Tj ET");
        }

        private static string CleanPdfText(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Not provided";
            }

            string cleaned = value.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ').Trim();
            while (cleaned.Contains("  "))
            {
                cleaned = cleaned.Replace("  ", " ");
            }

            // Keep the certificate layout readable if a title is unusually long.
            if (cleaned.Length > 85)
            {
                cleaned = cleaned.Substring(0, 82) + "...";
            }

            return cleaned;
        }

        private static string EscapePdfString(string value)
        {
            return value.Replace("\\", "\\\\")
                        .Replace("(", "\\(")
                        .Replace(")", "\\)");
        }

        private static Encoding GetPdfTextEncoding()
        {
            // Helvetica with WinAnsiEncoding supports common Western characters.
            // Characters outside that encoding are replaced with '?'.
            return Encoding.GetEncoding(1252);
        }

        private static void WriteAscii(Stream stream, string value)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}
