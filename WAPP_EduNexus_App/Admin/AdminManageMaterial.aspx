<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/AdminHeader_Footer.Master" AutoEventWireup="true" CodeBehind="AdminManageMaterial.aspx.cs" Inherits="WAPP_EduNexus_App.AdminManageMaterial" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder2" runat="server">
    <link href="<%= ResolveUrl("~/Content/QuizModule.css") %>" rel="stylesheet" />

    <div class="page-container">
        <div class="qz-page">
            <asp:HyperLink ID="lnkBack" runat="server" CssClass="qz-back" Text="&larr; Back to modules" />
            <h1 class="qz-title">Learning materials</h1>
            <p class="qz-sub">Module: <strong><asp:Literal ID="ltModuleTitle" runat="server" /></strong></p>

            <asp:Label ID="lblMessage" runat="server" Visible="false" EnableViewState="false" />

            <!-- Add / edit form -->
            <div class="qz-card">
                <h2 class="qz-card-title"><asp:Literal ID="ltFormTitle" runat="server" /></h2>
                <asp:HiddenField ID="hfMaterialId" runat="server" />

                <div class="qz-row">
                    <div class="qz-field">
                        <label for="<%= txtTitle.ClientID %>">Title</label>
                        <asp:TextBox ID="txtTitle" runat="server" CssClass="qz-input" MaxLength="150" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtTitle" ValidationGroup="vgMaterial"
                            Display="Dynamic" CssClass="qz-error" ErrorMessage="Title is required." />
                    </div>
                    <div class="qz-field">
                        <label for="<%= ddlKind.ClientID %>">Type</label>
                        <asp:DropDownList ID="ddlKind" runat="server" CssClass="qz-input">
                            <asp:ListItem Value="Video">Video (MP4)</asp:ListItem>
                            <asp:ListItem Value="Reading">Reading material</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="qz-field">
                        <label for="<%= txtSortOrder.ClientID %>">Order in module</label>
                        <asp:TextBox ID="txtSortOrder" runat="server" CssClass="qz-input" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtSortOrder" ValidationGroup="vgMaterial"
                            Display="Dynamic" CssClass="qz-error" ErrorMessage="Order is required." />
                        <asp:RangeValidator runat="server" ControlToValidate="txtSortOrder" ValidationGroup="vgMaterial"
                            Type="Integer" MinimumValue="1" MaximumValue="999" Display="Dynamic" CssClass="qz-error"
                            ErrorMessage="Enter a whole number from 1 to 999." />
                    </div>
                </div>

                <div class="qz-field">
                    <label for="<%= fuFile.ClientID %>">File</label>
                    <asp:FileUpload ID="fuFile" runat="server" />
                    <span class="qz-help">Video: MP4 only. Reading: optional PDF. When editing, leave this empty to keep the current file.</span>
                </div>

                <div class="qz-field">
                    <label for="<%= txtReading.ClientID %>">Reading text (for reading material)</label>
                    <asp:TextBox ID="txtReading" runat="server" CssClass="qz-input" TextMode="MultiLine" Rows="8" />
                    <span class="qz-help">Type the lesson text here, upload a PDF above, or use both.</span>
                </div>

                <asp:Button ID="btnSave" runat="server" Text="Add material" CssClass="qz-btn qz-btn-primary"
                    ValidationGroup="vgMaterial" OnClick="btnSave_Click" />
                <asp:Button ID="btnCancel" runat="server" Text="Cancel edit" CssClass="qz-btn qz-btn-light"
                    CausesValidation="false" Visible="false" OnClick="btnCancel_Click" />
            </div>

            <!-- Material list -->
            <div class="qz-card">
                <h2 class="qz-card-title">Materials in this module</h2>
                <asp:Literal ID="ltEmpty" runat="server" />

                <asp:Repeater ID="rptMaterials" runat="server" OnItemCommand="rptMaterials_ItemCommand">
                    <HeaderTemplate>
                        <table class="qz-table">
                            <thead>
                                <tr>
                                    <th>Order</th>
                                    <th>Type</th>
                                    <th>Title</th>
                                    <th>Content</th>
                                    <th></th>
                                </tr>
                            </thead>
                            <tbody>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td><%# Eval("sortOrder") %></td>
                            <td><span class='qz-tag qz-tag-<%# Eval("materialKind") %>'><%# Eval("materialKind") %></span></td>
                            <td><%#: Eval("materialTitle") %></td>
                            <td><%#: Summary(Container.DataItem) %></td>
                            <td class="qz-actions">
                                <asp:Button runat="server" Text="Edit" CssClass="qz-btn qz-btn-light"
                                    CommandName="EditMaterial" CommandArgument='<%# Eval("moduleMaterialID") %>' />
                                <asp:Button runat="server" Text="Delete" CssClass="qz-btn qz-btn-danger"
                                    CommandName="DeleteMaterial" CommandArgument='<%# Eval("moduleMaterialID") %>'
                                    OnClientClick="return confirm('Delete this material?');" />
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
