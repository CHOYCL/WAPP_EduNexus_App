<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LoginPage.aspx.cs" Inherits="WAPP_EduNexus_App.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
     <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Login | EduNexus</title>
    <link href="https://fonts.googleapis.com/css2?family=Poppins:wght@400;600;700&display=swap" rel="stylesheet" />
    <link href="LoginRegister.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">

        <div class="logincard">

            <div class="logoleft">
                <img src="logo.png" alt="EduNexus logo" class="logo" />
                <p class="caption">Your learning journey starts here.</p>
            </div>

            <!-- RIGHT SIDE: login form -->
            <div class="loginform">

                <p class="welcome">Welcome</p>
                <h1>Log in now</h1>

                <div class="field">
                    <asp:Label ID="LabelUsername" runat="server" Text="Username" CssClass="label"></asp:Label>
                    <asp:TextBox ID="txtUsername" runat="server" CssClass="input" placeholder="Enter your username"></asp:TextBox>
                </div>

                <div class="field">
                    <asp:Label ID="LabelPassword" runat="server" Text="Password" CssClass="label"></asp:Label>
                    <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="input" placeholder="Enter your password"></asp:TextBox>
                </div>

                <div class="field">
                    <asp:Label ID="errorMsg" runat="server" Visible="False" CssClass="error"></asp:Label>
                </div>

                <p class="registertext">
                    Don't have an account? <a href="RegisterPage.aspx">Register</a>
                </p>

                <div class="buttoncontainer">
                    <asp:Button ID="btnLogin" runat="server" Text="Login" CssClass="button" OnClick="btnLogin_Click" />
                    <a href="RegisterPage.aspx" class="switch-link">Register</a>
                </div>

            </div>

        </div>

    </form>
</body>
</html>