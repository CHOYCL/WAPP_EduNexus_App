<%@ Page Title="Profile | EduNexus" Language="C#" MasterPageFile="~/MasterPages/HeaderTopBar_Footer.Master"
    AutoEventWireup="true" CodeBehind="EditProfile.aspx.cs" Inherits="WAPP_EduNexus_App.CommonPage.EditProfile" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="EditProfile.css" rel="stylesheet" />
</asp:Content>


<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="profilepage">
        <div class="profilecard">

            <asp:Label ID="lblTitle" runat="server" CssClass="profiletitle" Text="My Profile"></asp:Label>
            <br />
            <asp:Label ID="lblMessage" runat="server" CssClass="profilemessage" Visible="False"></asp:Label>

             <!-- View Only-->
            <asp:Panel ID="pnlView" runat="server">
                <table style="width:100%;">
                    <tr>
                        <td class="profilelabel">&nbsp;</td>
                        <td class="profilevalue">
                            <asp:Image ID="imgProfile" runat="server" CssClass="profileavatar" />
                        </td>
                    </tr>
                    <tr>
                        <td class="profilelabel">
                            <asp:Label ID="Label1" runat="server" Text="Username"></asp:Label>
                        </td>
                        <td class="profilevalue">
                            <asp:Label ID="lblUsername" runat="server" Text="Label"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td class="profilelabel">
                            <asp:Label ID="Label2" runat="server" Text="Email"></asp:Label>
                        </td>
                        <td class="profilevalue">
                            <asp:Label ID="lblEmail" runat="server" Text="Label"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td class="profilelabel">
                            <asp:Label ID="Label3" runat="server" Text="Phone Number"></asp:Label>
                        </td>
                        <td class="profilevalue">
                            <asp:Label ID="lblPhone" runat="server" Text="Label"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td class="profilelabel">
                            <asp:Label ID="Label4" runat="server" Text="Password"></asp:Label>
                        </td>
                        <td class="profilevalue">********</td>
                    </tr>
                    <tr>
                        <td class="profilelabel">
                            <asp:Button ID="btnEdit" runat="server" Text="Edit Profile" CssClass="profilebutton" OnClick="btnEdit_Click" />
                        </td>
                        <td class="profilevalue">&nbsp;</td>
                    </tr>
                </table>
            </asp:Panel>

            <!-- Edit-->
            <asp:Panel ID="pnlEdit" runat="server" Visible="False">
                <table style="width:100%;">
                    <tr>
                        <td class="profilelabel">
                            <asp:Label ID="Label5" runat="server" Text="Profile Picture"></asp:Label>
                        </td>
                        <td class="profilevalue">
                            <asp:Image ID="imgEdit" runat="server" CssClass="profileavatar" />
                            <br />
                            <asp:FileUpload ID="UploadPic" runat="server" />
                        </td>
                    </tr>
                    <tr>
                        <td class="profilelabel">
                            <asp:Label ID="Label6" runat="server" Text="Username"></asp:Label>
                        </td>
                        <td class="profilevalue">
                            <asp:TextBox ID="txtUsername" runat="server" CssClass="profileinput"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td class="profilelabel">
                            <asp:Label ID="Label7" runat="server" Text="Email"></asp:Label>
                        </td>
                        <td class="profilevalue">
                            <asp:TextBox ID="txtEmail" runat="server" CssClass="profileinput"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td class="profilelabel">
                            <asp:Label ID="Label8" runat="server" Text="Phone Number"></asp:Label>
                        </td>
                        <td class="profilevalue">
                            <asp:TextBox ID="txtPhone" runat="server" CssClass="profileinput"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td class="profilelabel">
                            <asp:Label ID="Label9" runat="server" Text="New Password"></asp:Label>
                        </td>
                        <td class="profilevalue">
                            <asp:TextBox ID="txtNewPassword" runat="server" TextMode="Password" CssClass="profileinput"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td class="profilelabel">
                            <asp:Label ID="Label10" runat="server" Text="Confirm Password"></asp:Label>
                        </td>
                        <td class="profilevalue">
                            <asp:TextBox ID="txtConfirmPassword" runat="server" TextMode="Password" CssClass="profileinput"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td class="profilelabel">&nbsp;</td>
                        <td class="profilevalue">
                            <small>Leave the password boxes empty to keep your current password.</small>
                        </td>
                    </tr>
                    <tr>
                        <td class="profilelabel">
                            <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="profilebutton" OnClick="btnSave_Click" />
                        </td>
                        <td class="profilevalue">
                            <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="profilecancel" CausesValidation="False" OnClick="btnCancel_Click" />
                        </td>
                    </tr>
                </table>
            </asp:Panel>

        </div>
    </div>

</asp:Content>