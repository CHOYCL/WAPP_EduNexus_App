<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/HeaderTopBar_Footer.Master" AutoEventWireup="true" CodeBehind="CourseDetail.aspx.cs" Inherits="WAPP_EduNexus_App.CourseDetail" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div>
        <div class="breadcrumb">
            <a href="BrowseCourse.aspx">Catalog</a>

            <span class="breadcrumb-arrow">›</span>

            <asp:Label
                ID="lblBreadcrumbCourse"
                runat="server"
                CssClass="breadcrumb-current">
            
            </asp:Label>
            <asp:Label ID="lblCourseTitle" runat="server" />
            <asp:Label ID="lblCategory" runat="server" />
            <asp:Label ID="lblLevel" runat="server" />
            <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:ConnectionString %>" OnSelecting="SqlDataSource1_Selecting" ProviderName="<%$ ConnectionStrings:ConnectionString.ProviderName %>" SelectCommand="SELECT * FROM [CourseDetailTable]"></asp:SqlDataSource>
        </div>
        <div></div>
    </div>
</asp:Content>
