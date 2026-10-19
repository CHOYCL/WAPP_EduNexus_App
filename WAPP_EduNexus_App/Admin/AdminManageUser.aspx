<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/AdminHeader_Footer.Master" AutoEventWireup="true" CodeBehind="AdminManageUser.aspx.cs" Inherits="WAPP_EduNexus_App.Admin.AdminManageUser" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="AdminManageUser.css" rel="stylesheet" />
</asp:Content>


<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder2" runat="server">

    <div class="manageusers">

        <h1 class="manageuserstitle">Manage Users</h1>
        <p class="manageuserstext">View all registered users and remove accounts when needed.</p>

        <asp:Label ID="lblCount" runat="server" CssClass="usercount"></asp:Label>
        <asp:Label ID="lblMessage" runat="server" CssClass="usermessage" Visible="False"></asp:Label>

        <div class="usergrid">

            <asp:Repeater ID="rptUsers" runat="server" OnItemCommand="rptUsers_ItemCommand">
                <ItemTemplate>

                    <div class="usercard">

                        <asp:Image ID="imgUser" runat="server" CssClass="useravatar" ImageUrl='<%# GetUserPic(Eval("ProfilePicture")) %>' />

                        <h3 class="usercardname"><%# Eval("Username") %></h3>

                        <span class='userrole <%# Eval("Role").ToString().ToLower() %>'><%# Eval("Role") %></span>

                        <p class="usercardinfo"><%# Eval("Email") %></p>
                        <p class="usercardinfo">Phone: <%# Eval("PhoneNumber").ToString().Trim() %></p>

                        <asp:Button ID="btnDelete" runat="server" Text="Delete User" CssClass="userdelete"
                            CommandName="DeleteUser" CommandArgument='<%# Eval("Id") %>'
                            OnClientClick="return confirm('Are you sure you want to delete this user? This cannot be undone.');" />

                    </div>

                </ItemTemplate>
            </asp:Repeater>

        </div>

    </div>

</asp:Content>