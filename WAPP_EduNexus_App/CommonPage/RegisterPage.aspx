<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RegisterPage.aspx.cs" Inherits="WAPP_EduNexus_App.CommonPage.RegisterPage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
     <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Register | EduNexus</title>
    <link href="https://fonts.googleapis.com/css2?family=Poppins:wght@400;600;700&display=swap" rel="stylesheet" />
    <link href="LoginRegister.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">

        <div class="logincard">

            <!-- LEFT SIDE: logo -->
            <div class="logoleft">
                <img src="logo.png" alt="EduNexus logo" class="logo" />
                <p class="caption">Join EduNexus today.</p>
            </div>

            <!-- RIGHT SIDE: register form -->
            <div class="loginform">

                <p class="welcome">Welcome</p>
                <h1>Create account</h1>

                <div class="field">
                    <asp:Label ID="LabelUsername" runat="server" Text="Username" CssClass="label"></asp:Label>
                    <asp:TextBox ID="txtUsername" runat="server" CssClass="input" placeholder="Choose a username"></asp:TextBox>
                </div>

                <div class="field">
                    <asp:Label ID="LabelEmail" runat="server" Text="Email" CssClass="label"></asp:Label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="input" placeholder="Enter your email"></asp:TextBox>
                </div>

                <div class="field">
                    <asp:Label ID="LabelPassword" runat="server" Text="Password" CssClass="label"></asp:Label>
                    <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="input" placeholder="Create a password"></asp:TextBox>
                </div>

                <div class="field">
                    <asp:Label ID="LabelConfirm" runat="server" Text="Confirm Password" CssClass="label"></asp:Label>
                    <asp:TextBox ID="txtConfirm" runat="server" TextMode="Password" CssClass="input" placeholder="Repeat your password"></asp:TextBox>
                </div>

                <div class="field">
                    <asp:Label ID="errorMsg" runat="server" Visible="False" CssClass="error"></asp:Label>
                </div>

                <p class="registertext">
                    Already have an account? <a href="LoginPage.aspx">Login</a>
                </p>

                <div class="buttoncontainer">
                    <asp:Button ID="btnRegister" runat="server" Text="Register" CssClass="button" OnClick="btnRegister_Click" />
                    <a href="LoginPage.aspx" class="switch-link">Login</a>
                </div>

            </div>

        </div>

    </form>
</body>
</html>