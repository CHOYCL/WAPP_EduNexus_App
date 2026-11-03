<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/HeaderTopBar_Footer.Master" AutoEventWireup="true" CodeBehind="QuizResult.aspx.cs" Inherits="WAPP_EduNexus_App.QuizResult" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <link href="<%= ResolveUrl("~/Content/QuizModule.css") %>" rel="stylesheet" />

    <div class="page-container">
        <div class="qz-page">
            <asp:HyperLink ID="lnkBack" runat="server" CssClass="qz-back" Text="&larr; Back to module" NavigateUrl="~/CommonPage/Home.aspx" />
            <h1 class="qz-title">Quiz result</h1>

            <asp:Label ID="lblMessage" runat="server" Visible="false" EnableViewState="false" />

            <asp:Panel ID="pnlResult" runat="server">
                <div class="qz-card">
                    <asp:Literal ID="ltSummary" runat="server" />
                    <asp:HyperLink ID="lnkRetake" runat="server" CssClass="qz-btn qz-btn-primary" Visible="false" />
                </div>

                <h2 class="qz-card-title">Your answers</h2>
                <asp:Repeater ID="rptReview" runat="server">
                    <ItemTemplate>
                        <%# RenderReview(Container.DataItem) %>
                    </ItemTemplate>
                </asp:Repeater>
            </asp:Panel>
        </div>
    </div>
</asp:Content>
