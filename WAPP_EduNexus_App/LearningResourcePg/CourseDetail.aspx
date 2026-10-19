
<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/HeaderTopBar_Footer.Master" AutoEventWireup="true" CodeBehind="CourseDetail.aspx.cs" Inherits="WAPP_EduNexus_App.CourseDetail" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

<link href="../Admin/AdminManageCourseDetail.css" rel="stylesheet" />

    <div class="page-container">
        <section class="Section1">

            <!-- Course image or video -->
            <div class="contentIMG_VID">
                <div class="media-placeholder">

                    <asp:Image ID="imgCourseMedia"
                        runat="server"
                        CssClass="courseMedia"
                        Visible="false" />

                    <video class="courseMedia"
                        id="courseVID"
                        runat="server"
                        controls
                        style="display:none;">

                        <source id="videoSource"
                            runat="server"
                            src="#"
                            type="video/mp4" />

                        Your browser does not support the video tag.
                    </video>

                </div>
            </div>

            <!-- Course information: view only -->
            <div class="content-description">

                <p class="titleNaming">COURSE TITLE</p>

                <h1 class="course-title">
                    <asp:Literal ID="ltCourseTitle" runat="server" />
                </h1>

                <p class="titleNaming">DESCRIPTION</p>

                <p class="course-desc">
                    <asp:Literal ID="ltMaterialDescription" runat="server" />
                </p>

                <!-- Category and level -->
                <div class="tag-row">
                    <asp:Literal ID="ltCategory" runat="server" />
                    <asp:Literal ID="ltLevel" runat="server" />
                </div>

                <!-- Instructor -->
                <div class="tag-row">
                    <asp:Literal ID="ltInstructor" runat="server" />
                </div>

            </div>
        </section>
    </div>

</asp:Content>
