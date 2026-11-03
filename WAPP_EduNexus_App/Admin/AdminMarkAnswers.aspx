<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/AdminHeader_Footer.Master" AutoEventWireup="true" CodeBehind="AdminMarkAnswers.aspx.cs" Inherits="WAPP_EduNexus_App.AdminMarkAnswers" %>
<%@ Import Namespace="WAPP_EduNexus_App" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder2" runat="server">
    <link href="<%= ResolveUrl("~/Content/QuizModule.css") %>" rel="stylesheet" />

    <div class="page-container">
        <div class="qz-page">
            <asp:HyperLink ID="lnkBack" runat="server" CssClass="qz-back" Text="&larr; Back to modules" />
            <h1 class="qz-title">Quiz attempts</h1>
            <p class="qz-sub">Module: <strong><asp:Literal ID="ltModuleTitle" runat="server" /></strong></p>

            <asp:Label ID="lblMessage" runat="server" Visible="false" EnableViewState="false" />

            <!-- Marking panel (shown when an attempt is selected) -->
            <asp:Panel ID="pnlMark" runat="server" CssClass="qz-card" Visible="false">
                <h2 class="qz-card-title"><asp:Literal ID="ltMarkHeader" runat="server" /></h2>
                <asp:Literal ID="ltNoLong" runat="server" />

                <asp:Repeater ID="rptAnswers" runat="server">
                    <ItemTemplate>
                        <div class="qz-card">
                            <asp:HiddenField ID="hfAnswerId" runat="server" Value='<%# Eval("answerID") %>' />
                            <p class="qz-q-head"><%#: Eval("questionText") %>
                                <span class="qz-muted">(out of <%# Eval("marks") %>)</span></p>
                            <p class="qz-muted">Marking guide: <%#: Eval("modelAnswer") %></p>
                            <div class="qz-student-answer"><%#: Eval("answerText") %></div>

                            <div class="qz-row">
                                <div class="qz-field">
                                    <label>Marks awarded</label>
                                    <asp:TextBox ID="txtAwarded" runat="server" CssClass="qz-input qz-input-sm" Text='<%# Eval("AwardedText") %>' />
                                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtAwarded" ValidationGroup="vgMark"
                                        Display="Dynamic" CssClass="qz-error" ErrorMessage="Enter the marks." />
                                    <asp:RangeValidator runat="server" ControlToValidate="txtAwarded" ValidationGroup="vgMark"
                                        Type="Double" MinimumValue="0" MaximumValue='<%# Eval("marks") %>'
                                        Display="Dynamic" CssClass="qz-error" ErrorMessage="Marks must be between 0 and the maximum." />
                                </div>
                                <div class="qz-field">
                                    <label>Feedback for the student (optional)</label>
                                    <asp:TextBox ID="txtFeedback" runat="server" CssClass="qz-input" MaxLength="500" Text='<%# Eval("FeedbackText") %>' />
                                </div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>

                <asp:Button ID="btnSaveMarks" runat="server" Text="Save marks" CssClass="qz-btn qz-btn-primary"
                    ValidationGroup="vgMark" OnClick="btnSaveMarks_Click" />
            </asp:Panel>

            <!-- Attempts list -->
            <div class="qz-card">
                <h2 class="qz-card-title">All attempts (waiting for marking first)</h2>
                <asp:Literal ID="ltAttemptsEmpty" runat="server" />

                <asp:Repeater ID="rptAttempts" runat="server">
                    <HeaderTemplate>
                        <table class="qz-table">
                            <thead>
                                <tr>
                                    <th>Student</th>
                                    <th>Attempt</th>
                                    <th>Date</th>
                                    <th>Status</th>
                                    <th>Score</th>
                                    <th>Percentage</th>
                                    <th>Grade</th>
                                    <th></th>
                                </tr>
                            </thead>
                            <tbody>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td><%#: Eval("Username") %></td>
                            <td><%# Eval("attemptNo") %> of 3</td>
                            <td><%# Eval("attemptDate", "{0:dd MMM yyyy HH:mm}") %></td>
                            <td><span class='qz-tag qz-tag-<%# Eval("status") %>'><%# Eval("status") %></span></td>
                            <td><%# QuizHelper.Mark(Eval("score")) %> / <%# Eval("totalMarks") %></td>
                            <td><%# Pct(Eval("percentage")) %></td>
                            <td><%# QuizHelper.Dash(Eval("grade")) %></td>
                            <td class="qz-links">
                                <a href='AdminMarkAnswers.aspx?moduleId=<%# Eval("moduleID") %>&attemptId=<%# Eval("attemptID") %>'>
                                    <%# Convert.ToString(Eval("status")) == "Pending" ? "Mark answers" : "Review marks" %>
                                </a>
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
