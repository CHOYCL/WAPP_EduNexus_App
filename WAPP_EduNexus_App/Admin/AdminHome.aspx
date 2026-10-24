<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/AdminHeader_Footer.Master" AutoEventWireup="true" CodeBehind="AdminHome.aspx.cs" Inherits="WAPP_EduNexus_App.Admin.AdminHome" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="AdminHome.css" rel="stylesheet" />
</asp:Content>


<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder2" runat="server">

    <div class="dashpage">

        <h1 class="dashtitle">Admin Dashboard</h1>
        <asp:Label ID="lblWelcome" runat="server" CssClass="dashwelcome"></asp:Label>

        <!-- ===== NUMBER CARDS ===== -->
        <div class="dashstats">

            <div class="dashstat">
                <p class="dashstatlabel">TOTAL USERS</p>
                <asp:Label ID="lblTotal" runat="server" CssClass="dashstatnumber" Text="0"></asp:Label>
            </div>

            <div class="dashstat">
                <p class="dashstatlabel">STUDENTS</p>
                <asp:Label ID="lblStudents" runat="server" CssClass="dashstatnumber" Text="0"></asp:Label>
            </div>

            <div class="dashstat">
                <p class="dashstatlabel">ADMINS</p>
                <asp:Label ID="lblAdmins" runat="server" CssClass="dashstatnumber" Text="0"></asp:Label>
            </div>

        </div>

        <!-- ===== SHORTCUTS ===== -->
        <h2 class="dashsubtitle">Quick Actions</h2>

        <div class="dashactions">

            <a href="AdminManageCourse.aspx" class="dashaction">
                <span class="dashicon">📚</span>
                <strong>Manage Courses</strong>
                <span>Add, edit and organise courses.</span>
            </a>

            <a href="AdminManageUser.aspx" class="dashaction">
                <span class="dashicon">👥</span>
                <strong>Manage Users</strong>
                <span>View and remove user accounts.</span>
            </a>

            <a href="AdminManageDiscussion.aspx" class="dashaction">
                <span class="dashicon">💬</span>
                <strong>Manage Discussion</strong>
                <span>Moderate course discussions.</span>
            </a>

        </div>

        <!-- ===== NEWEST USERS ===== -->
        <h2 class="dashsubtitle">Newest Users</h2>

        <div class="dashtablebox">
            <asp:GridView ID="gvNewUsers" runat="server" AutoGenerateColumns="False" GridLines="None" CssClass="dashtable">
                <Columns>
                    <asp:BoundField DataField="Username" HeaderText="Username" />
                    <asp:BoundField DataField="Email" HeaderText="Email" />
                    <asp:BoundField DataField="Role" HeaderText="Role" />
                </Columns>
            </asp:GridView>
        </div>

    </div>

</asp:Content>