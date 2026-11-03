<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/HeaderTopBar_Footer.Master" AutoEventWireup="true" CodeBehind="TakeQuiz.aspx.cs" Inherits="WAPP_EduNexus_App.TakeQuiz" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <link href="<%= ResolveUrl("~/Content/QuizModule.css") %>" rel="stylesheet" />

    <div class="page-container">
        <div class="qz-page">
            <asp:HyperLink ID="lnkBack" runat="server" CssClass="qz-back" Text="&larr; Back to module" NavigateUrl="~/CommonPage/Home.aspx" />
            <h1 class="qz-title"><asp:Literal ID="ltModuleTitle" runat="server" /> quiz</h1>
            <p class="qz-sub"><asp:Literal ID="ltInfo" runat="server" /></p>

            <asp:Label ID="lblMessage" runat="server" Visible="false" EnableViewState="false" />

            <asp:Panel ID="pnlQuiz" runat="server">
                <asp:Repeater ID="rptQuestions" runat="server" OnItemDataBound="rptQuestions_ItemDataBound">
                    <ItemTemplate>
                        <div class="qz-card qz-question">
                            <asp:HiddenField ID="hfQuestionId" runat="server" Value='<%# Eval("questionID") %>' />
                            <p class="qz-q-head">
                                <strong><%# Container.ItemIndex + 1 %>.</strong>
                                <%#: Eval("questionText") %>
                                <span class="qz-muted">(<%# Eval("marks") %> mark<%# Convert.ToInt32(Eval("marks")) == 1 ? "" : "s" %>)</span>
                            </p>

                            <asp:RadioButtonList ID="rblOptions" runat="server" CssClass="qz-options" />
                            <asp:TextBox ID="txtShort" runat="server" CssClass="qz-input" MaxLength="250" placeholder="Type your answer" />
                            <asp:TextBox ID="txtLong" runat="server" CssClass="qz-input" TextMode="MultiLine" Rows="6" placeholder="Write your answer" />
                        </div>
                    </ItemTemplate>
                </asp:Repeater>

                <asp:Button ID="btnSubmit" runat="server" Text="Submit answers" CssClass="qz-btn qz-btn-primary"
                    OnClick="btnSubmit_Click"
                    OnClientClick="return confirm('Submit your answers? This uses one of your 3 attempts.');" />
            </asp:Panel>
        </div>
    </div>
</asp:Content>
