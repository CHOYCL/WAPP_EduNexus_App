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
                <a href="../LearningResourcePg/CourseDetail.aspx" class="Course-Card">
                    <div class="CourseCard">
                        <img src="../IMG/PhotographyCardPhoto.jpg"
                             class="course-image"
                             alt="Night Photography" />

                        <div class="course-content">
                            <h2>Night Photography</h2>

                            <p>
                                Learn to see, light, and pose. This course walks
                                through natural + studio setups and shows you...
                            </p>

                            <div class="course-info">
                                <span>3 lessons</span>
                                <span><svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" aria-hidden="true" role="img" width="18" height="18" viewBox="0 0 24 24" style="color: rgb(74, 85, 101); opacity: 1; transform: rotate(0deg);"><path fill="currentColor" fill-rule="evenodd" d="M12 6a3.5 3.5 0 1 0 0 7a3.5 3.5 0 0 0 0-7m-1.5 8a4 4 0 0 0-4 4a2 2 0 0 0 2 2h7a2 2 0 0 0 2-2a4 4 0 0 0-4-4zm6.82-3.096a5.51 5.51 0 0 0-2.797-6.293a3.5 3.5 0 1 1 2.796 6.292ZM19.5 18h.5a2 2 0 0 0 2-2a4 4 0 0 0-4-4h-1.1a5.5 5.5 0 0 1-.471.762A6 6 0 0 1 19.5 18M4 7.5a3.5 3.5 0 0 1 5.477-2.889a5.5 5.5 0 0 0-2.796 6.293A3.5 3.5 0 0 1 4 7.5M7.1 12H6a4 4 0 0 0-4 4a2 2 0 0 0 2 2h.5a6 6 0 0 1 3.071-5.238A5.5 5.5 0 0 1 7.1 12" clip-rule="evenodd"></path></svg>0</span>
                            </div>

                            <hr />

                            <div class="course-bottom">
                                <span>PHOTOGRAPHY</span>
                                <span>Intermediate</span>
                            </div>
                        </div>
                    </div>
                </a>
            </div>
        </section>
    </div>
</asp:Content>
