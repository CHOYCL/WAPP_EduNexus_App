<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/HeaderTopBar_Footer.Master" AutoEventWireup="true" CodeBehind="BrowseCourse.aspx.cs" Inherits="WAPP_EduNexus_App.BrowseCourse" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <link href="BrowseCourse.css" rel="stylesheet" />
    <div class="page-container">
        <section class="CourseCatalogBar">
            <div class="CourseCatalogTitle">
                <p>COURSE CATALOG</p>
                <h1>Find your next skill</h1>
            </div>
            <div class="search-container">
                <div class="search-box">
                    <svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" aria-hidden="true" role="img" width="25" height="25" viewBox="0 0 24 24" style="color: rgb(74, 85, 101); opacity: 1; transform: rotate(0deg);"><g fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2"><path d="m21 21l-4.34-4.34"></path><circle cx="11" cy="11" r="8"></circle></g></svg>
                    <asp:TextBox ID="txtSearch" runat="server" CssClass="search-input" placeholder="Search for courses..." />
                </div>

                <div class="dropdown-container">
                    <asp:DropDownList ID="ddlCategory" runat="server" CssClass="filter-dropdown">
                    <asp:ListItem Text="All categories" Value="" />
                    <asp:ListItem Text="Programming" Value="Programming" />
                    <asp:ListItem Text="Business" Value="Photography" />
                    <asp:ListItem Text="Design" Value="Design" />
                </asp:DropDownList>
                    <span class="dropdown-icon"><svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" aria-hidden="true" role="img" width="30" height="30" viewBox="0 0 24 24" style="color: rgb(74, 85, 101); opacity: 1; transform: rotate(0deg);"><path fill="currentColor" d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10s10-4.48 10-10S17.52 2 12 2m-.35 12.65l-2.79-2.79c-.32-.32-.1-.86.35-.86h5.59c.45 0 .67.54.35.85l-2.79 2.79c-.2.2-.52.2-.71.01"></path></svg></span>
                </div>

                <div class="dropdown-container">
                    <asp:DropDownList ID="ddlLevel" runat="server" CssClass="filter-dropdown">
                    <asp:ListItem Text="Any level" Value="" />
                    <asp:ListItem Text="Beginner" Value="Beginner" />
                    <asp:ListItem Text="Intermediate" Value="Intermediate" />
                    <asp:ListItem Text="Advanced" Value="Advanced" />
                </asp:DropDownList>
                    <span class="dropdown-icon"><svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" aria-hidden="true" role="img" width="30" height="30" viewBox="0 0 24 24" style="color: rgb(74, 85, 101); opacity: 1; transform: rotate(0deg);"><path fill="currentColor" d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10s10-4.48 10-10S17.52 2 12 2m-.35 12.65l-2.79-2.79c-.32-.32-.1-.86.35-.86h5.59c.45 0 .67.54.35.85l-2.79 2.79c-.2.2-.52.2-.71.01"></path></svg></span>
                </div>

                <asp:Button ID="btnApply" runat="server" Text="Apply" CssClass="apply-button" OnClick="btnApply_Click" />
            </div>
        </section>


<section class="CourseSection">
    <div class="CourseGrid">

        <asp:Literal ID="ltBrowseMessage" runat="server" />

        <asp:Repeater ID="rptCourses" runat="server">
            <ItemTemplate>

                <asp:HyperLink
                    ID="lnkCourse"
                    runat="server"
                    CssClass="Course-Card"
                    NavigateUrl='<%# ResolveUrl("~/LearningResourcePg/CourseDetail.aspx?courseId=" + Eval("courseID")) %>'>

                    <div class="CourseCard">

                        <img
                            src='<%# HttpUtility.HtmlAttributeEncode(Convert.ToString(Eval("ImageUrl"))) %>'
                            class="course-image"
                            alt='<%# HttpUtility.HtmlAttributeEncode(Convert.ToString(Eval("courseTitle"))) %>' />

                        <div class="course-content">

                            <h2>
                                <%# Server.HtmlEncode(Convert.ToString(Eval("courseTitle"))) %>
                            </h2>

                            <p>
                                <%# string.IsNullOrWhiteSpace(Convert.ToString(Eval("Description")))
                                    ? "Explore this course to build new skills."
                                    : Server.HtmlEncode(Convert.ToString(Eval("Description"))) %>
                            </p>

                            <div class="course-info">
                                <span>
                                    <%# Eval("LessonCount") %> lessons
                                </span>
                            </div>

                            <hr />

                            <div class="course-bottom">
                                <span>
                                    <%# string.IsNullOrWhiteSpace(Convert.ToString(Eval("Category")))
                                        ? "Uncategorized"
                                        : Server.HtmlEncode(Convert.ToString(Eval("Category"))) %>
                                </span>

                                <span>
                                    <%# string.IsNullOrWhiteSpace(Convert.ToString(Eval("Level")))
                                        ? "Not set"
                                        : Server.HtmlEncode(Convert.ToString(Eval("Level"))) %>
                                </span>
                            </div>

                        </div>
                    </div>

                </asp:HyperLink>

            </ItemTemplate>
        </asp:Repeater>

    </div>
</section>
    </div>
</asp:Content>
