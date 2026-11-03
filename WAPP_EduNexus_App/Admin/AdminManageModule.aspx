<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/AdminHeader_Footer.Master" AutoEventWireup="true" CodeBehind="AdminManageModule.aspx.cs" Inherits="WAPP_EduNexus_App.AdminManageModule" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder2" runat="server">
    <link href="<%= ResolveUrl("~/Content/QuizModule.css") %>" rel="stylesheet" />

    <div class="page-container">
        <div class="qz-page">
            <a class="qz-back" href="AdminManageCourse.aspx">&larr; Back to courses</a>
            <h1 class="qz-title">Modules</h1>
            <p class="qz-sub">Course: <strong><asp:Literal ID="ltCourseTitle" runat="server" /></strong></p>

            <asp:Label ID="lblMessage" runat="server" Visible="false" EnableViewState="false" />

            <!-- Add / edit form -->
            <div class="qz-card">
                <h2 class="qz-card-title"><asp:Literal ID="ltFormTitle" runat="server" /></h2>
                <asp:HiddenField ID="hfModuleId" runat="server" />

                <div class="qz-field">
                    <label for="<%= txtTitle.ClientID %>">Module title</label>
                    <asp:TextBox ID="txtTitle" runat="server" CssClass="qz-input" MaxLength="150" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtTitle" ValidationGroup="vgModule"
                        Display="Dynamic" CssClass="qz-error" ErrorMessage="Module title is required." />
                </div>

                <div class="qz-field">
                    <label for="<%= txtDescription.ClientID %>">Description</label>
                    <asp:TextBox ID="txtDescription" runat="server" CssClass="qz-input" TextMode="MultiLine" Rows="3" />
                </div>

                <div class="qz-row">
                    <div class="qz-field">
                        <label for="<%= txtSortOrder.ClientID %>">Order in course</label>
                        <asp:TextBox ID="txtSortOrder" runat="server" CssClass="qz-input" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtSortOrder" ValidationGroup="vgModule"
                            Display="Dynamic" CssClass="qz-error" ErrorMessage="Order is required." />
                        <asp:RangeValidator runat="server" ControlToValidate="txtSortOrder" ValidationGroup="vgModule"
                            Type="Integer" MinimumValue="1" MaximumValue="999" Display="Dynamic" CssClass="qz-error"
                            ErrorMessage="Enter a whole number from 1 to 999." />
                    </div>
                    <div class="qz-field">
                        <label for="<%= txtPassPercentage.ClientID %>">Pass mark (%)</label>
                        <asp:TextBox ID="txtPassPercentage" runat="server" CssClass="qz-input" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtPassPercentage" ValidationGroup="vgModule"
                            Display="Dynamic" CssClass="qz-error" ErrorMessage="Pass mark is required." />
                        <asp:RangeValidator runat="server" ControlToValidate="txtPassPercentage" ValidationGroup="vgModule"
                            Type="Integer" MinimumValue="1" MaximumValue="100" Display="Dynamic" CssClass="qz-error"
                            ErrorMessage="Enter a whole number from 1 to 100." />
                    </div>
                </div>

                <asp:Button ID="btnSave" runat="server" Text="Add module" CssClass="qz-btn qz-btn-primary"
                    ValidationGroup="vgModule" OnClick="btnSave_Click" />
                <asp:Button ID="btnCancel" runat="server" Text="Cancel edit" CssClass="qz-btn qz-btn-light"
                    CausesValidation="false" Visible="false" OnClick="btnCancel_Click" />
            </div>

            <!-- Module list -->
            <div class="qz-card">
                <h2 class="qz-card-title">Modules in this course</h2>
                <asp:Literal ID="ltEmpty" runat="server" />

                <asp:Repeater ID="rptModules" runat="server" OnItemCommand="rptModules_ItemCommand">
                    <HeaderTemplate>
                        <table class="qz-table">
                            <thead>
                                <tr>
                                    <th>Order</th>
                                    <th>Module</th>
                                    <th>Pass mark</th>
                                    <th>Materials</th>
                                    <th>Questions</th>
                                    <th>Manage</th>
                                    <th></th>
                                </tr>
                            </thead>
                            <tbody>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td><%# Eval("sortOrder") %></td>
                            <td><%#: Eval("moduleTitle") %></td>
                            <td><%# Eval("passPercentage") %>%</td>
                            <td><%# Eval("MaterialCount") %></td>
                            <td><%# Eval("QuestionCount") %></td>
                            <td class="qz-links">
                                <a href='AdminManageMaterial.aspx?moduleId=<%# Eval("moduleID") %>'>Materials</a>
                                <a href='AdminManageQuiz.aspx?moduleId=<%# Eval("moduleID") %>'>Questions</a>
                                <a href='AdminMarkAnswers.aspx?moduleId=<%# Eval("moduleID") %>'>Attempts</a>
                            </td>
                            <td class="qz-actions">
                                <asp:Button runat="server" Text="Edit" CssClass="qz-btn qz-btn-light"
                                    CommandName="EditModule" CommandArgument='<%# Eval("moduleID") %>' />
                                <asp:Button runat="server" Text="Delete" CssClass="qz-btn qz-btn-danger"
                                    CommandName="DeleteModule" CommandArgument='<%# Eval("moduleID") %>'
                                    OnClientClick="return confirm('Delete this module with its materials, questions and all student attempts?');" />
                            </td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                            </tbody>
                        </table>
                    </FooterTemplate>
                </asp:Repeater>
            </div>
        </div>
    </div>
</asp:Content>
