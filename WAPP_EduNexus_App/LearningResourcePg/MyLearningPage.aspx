<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/HeaderTopBar_Footer.Master" AutoEventWireup="true" CodeBehind="MyLearningPage.aspx.cs" Inherits="WAPP_EduNexus_App.MyLearningPage" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <link href="MyLearningPage.css" rel="stylesheet" />
    <asp:Literal ID="ltLearningMessage" runat="server" />
    <div class="page-container">
        <setion id="LearningPgHeading">
        <p>MY LEARNING</p>
        <h1 class="learning-user-greeting">
            <asp:Literal ID="ltUsername" runat="server" />
        </h1>
        </setion>

        <setion class="LearningPgSection">
            <div class="LearningPgBox">
                <span class="icon">
                    <svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" aria-hidden="true" role="img" width="40" height="40" viewBox="0 0 24 24" style="color: #B8CFFF; opacity: 1; transform: rotate(0deg);"><path fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 19a9 9 0 0 1 9 0a9 9 0 0 1 9 0M3 6a9 9 0 0 1 9 0a9 9 0 0 1 9 0M3 6v13m9-13v13m9-13v13"></path></svg>
                </span>
            <div >
                <p class="LearningPgBoxTitle">ENROLLED</p>
                <p class="LearningPgBoxNum"><asp:Literal ID="ltEnrolledCount" runat="server" Text="0" /></p>
            </div>
            </div>

            <div class="LearningPgBox">
                <span class="icon">
                    <svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" aria-hidden="true" role="img" width="40" height="40" viewBox="0 0 24 24" style="color: #B8CFFF; opacity: 1; transform: rotate(0deg);"><path fill="none" stroke="currentColor" stroke-linecap="round" stroke-width="2" d="M5.636 5.636A9 9 0 1 0 12 3m0 9L6 6m6-3v2m9 7h-2m-7 7v2m-7-9H3"></path></svg>
                </span>
            <div >
                <p class="LearningPgBoxTitle">IN PROGRESS</p>
                <p class="LearningPgBoxNum"><asp:Literal ID="ltInProgressCount" runat="server" Text="0" /></p>
            </div>
            </div>

            <div class="LearningPgBox">
                <span class="icon">
                    <svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" aria-hidden="true" role="img" width="40" height="40" viewBox="0 0 24 24" style="color: #B8CFFF; opacity: 1; transform: rotate(0deg);"><path fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 17v4l3-2l3 2v-4M13.957 4.275l-.323-.444a2.022 2.022 0 0 0-3.268 0l-.323.444L9.5 4.19A2.02 2.02 0 0 0 7.19 6.5l.085.543l-.444.323a2.02 2.02 0 0 0 0 3.268l.444.323l-.085.542A2.02 2.02 0 0 0 9.5 13.81l.543-.085l.323.444a2.022 2.022 0 0 0 3.268 0l.323-.444l.542.085a2.02 2.02 0 0 0 2.311-2.31l-.085-.543l.444-.323a2.022 2.022 0 0 0 0-3.268l-.444-.323l.085-.542A2.02 2.02 0 0 0 14.5 4.19z"></path></svg>
                </span>
            <div >
                <p class="LearningPgBoxTitle">CERTIFICATE</p>
                <p class="LearningPgBoxNum"><asp:Literal ID="ltCertificateCount" runat="server" Text="0" /></p>
            </div>
            </div>
        </setion>


        <section class="LearningPgSection2">
            <h2>Continue learning</h2>

            <!-- Show when the student has no courses in progress -->
            <asp:Panel ID="pnlNoProgress" runat="server"
                CssClass="ContinueLearningBox">

                <p>
                    <asp:Literal ID="ltNoProgressMessage"
                        runat="server"
                        Text="Nothing in progress" />
                </p>

                <asp:HyperLink ID="lnkBrowseCourses"
                    runat="server"
                    CssClass="BrowseCourseBtn"
                    NavigateUrl="~/LearningResourcePg/BrowseCourse.aspx"
                    Text="Browse courses" />

            </asp:Panel>

            <!-- Show courses whose enrollment status is In Progress -->
            <div class="ContinueLearningGrid">
                <asp:Repeater ID="rptInProgressCourses" runat="server">
                    <ItemTemplate>

                        <div class="ContinueCourseCard">
                            <p class="course-status">IN PROGRESS</p>

                            <h3>
                                <%# Server.HtmlEncode(
                                    Convert.ToString(Eval("courseTitle"))) %>
                            </h3>

                            <p>
                                Enrolled on:
                                <%# Eval("EnrollmentDate", "{0:dd MMM yyyy}") %>
                            </p>

                            <a class="BrowseCourseBtn"
                               href='<%# ResolveUrl(
                                   "~/LearningResourcePg/CourseDetail.aspx?courseId="
                                   + Eval("courseID")) %>'>
                                Continue learning
                            </a>
                        </div>

                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </section>



        <section class="LearningPgCompleted">
            <h2>Completed &amp; Certificates</h2>

            <asp:Panel ID="pnlNoCertificates" runat="server">
                <p>No certificates available yet.</p>
            </asp:Panel>

            <div class="cert_grid">
                <asp:Repeater ID="rptCertificates" runat="server">
                    <ItemTemplate>

                        <div class="CertificateBox">

                            <p>
                                <%# string.IsNullOrWhiteSpace(
                                    Convert.ToString(Eval("Category")))
                                    ? "COURSE CERTIFICATE"
                                    : Server.HtmlEncode(
                                        Convert.ToString(Eval("Category"))) %>
                            </p>

                            <h3>
                                <%# Server.HtmlEncode(
                                    Convert.ToString(Eval("courseTitle"))) %>
                            </h3>

                            <p>
                                Certificate No.:
                                <%# Eval("CertificateNumber") %>
                            </p>

                            <p>
                                Issued:
                                <%# Eval("DateCreated", "{0:dd MMM yyyy}") %>
                            </p>

                        </div>

                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </section>

    </div>
</asp:Content>
