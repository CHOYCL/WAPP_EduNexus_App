<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/HeaderTopBar_Footer.Master" AutoEventWireup="true" CodeBehind="ModuleLearn.aspx.cs" Inherits="WAPP_EduNexus_App.ModuleLearn" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <link href="<%= ResolveUrl("~/Content/QuizModule.css") %>" rel="stylesheet" />

    <div class="page-container">
        <div class="qz-page">
            <a class="qz-back" href="CourseDetail.aspx?courseId=<%= CourseId %>">&larr; Back to course</a>
            <h1 class="qz-title"><asp:Literal ID="ltCourseTitle" runat="server" /></h1>

            <asp:Label ID="lblMessage" runat="server" Visible="false" EnableViewState="false" />

            <asp:Panel ID="pnlLearn" runat="server">
                <div class="qz-layout">

                    <!-- Module list -->
                    <aside class="qz-sidebar">
                        <h3>Modules</h3>
                        <asp:Repeater ID="rptModules" runat="server">
                            <ItemTemplate>
                                <a class='qz-module-link <%# IsActive(Eval("moduleID")) %>'
                                   href='ModuleLearn.aspx?courseId=<%# CourseId %>&moduleId=<%# Eval("moduleID") %>'>
                                    <span class="qz-module-no"><%# Container.ItemIndex + 1 %></span>
                                    <span class="qz-module-name"><%#: Eval("moduleTitle") %></span>
                                    <%# StatusBadge(Eval("AttemptsUsed"), Eval("PassedCount"), Eval("PendingCount")) %>
                                </a>
                            </ItemTemplate>
                        </asp:Repeater>
                    </aside>

                    <!-- Selected module -->
                    <section>
                        <h2 class="qz-card-title"><asp:Literal ID="ltModuleTitle" runat="server" /></h2>
                        <p class="qz-muted"><asp:Literal ID="ltModuleDesc" runat="server" /></p>

                        <asp:Literal ID="ltNoMaterials" runat="server" />
                        <asp:Repeater ID="rptMaterials" runat="server">
                            <ItemTemplate>
                                <%# RenderMaterial(Container.DataItem) %>
                            </ItemTemplate>
                        </asp:Repeater>

                        <div class="qz-card">
                            <h3 class="qz-card-title">Module quiz</h3>
                            <asp:Literal ID="ltQuizInfo" runat="server" />
                            <p>
                                <asp:HyperLink ID="lnkStartQuiz" runat="server" CssClass="qz-btn qz-btn-primary" Visible="false" />
                                <asp:HyperLink ID="lnkViewResult" runat="server" CssClass="qz-btn qz-btn-light" Visible="false" Text="View latest result" />
                            </p>
                        </div>
                    </section>

                </div>
            </asp:Panel>
        </div>
    </div>
</asp:Content>
