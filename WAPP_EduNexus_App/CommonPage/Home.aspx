<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/HeaderTopBar_Footer.Master" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="WAPP_EduNexus_App.CommonPage.Home" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="Home.css" rel="stylesheet" />
</asp:Content>


<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="homepage">
        <div class="homewrap">

            <!--  HERO  -->
            <div class="homehero">

                <div class="homeherotext">
                    <p class="homelabel">LEARN &bull; CONNECT &bull; GROW</p>
                    <h1 class="homeheadline">Learn At Your Own Pace with <span class="homehighlight">EduNexus</span></h1>
                    <p class="homesubtext">
                        A platform for people who love learning. Explore courses in design,
                        music, cooking, code and more, taught by experienced instructors.
                    </p>

                    <asp:Panel ID="pnlSearch" runat="server" CssClass="homesearch" DefaultButton="btnSearch">
                        <asp:TextBox ID="txtSearch" runat="server" CssClass="homesearchbox" placeholder="Search for courses..."></asp:TextBox>
                        <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="homebutton" OnClick="btnSearch_Click" />
                    </asp:Panel>

                    <p class="hometags">
                        Popular:
                        <a href="~/LearningResourcePg/BrowseCourse.aspx" runat="server">Design</a>
                        <a href="~/LearningResourcePg/BrowseCourse.aspx" runat="server">Music</a>
                        <a href="~/LearningResourcePg/BrowseCourse.aspx" runat="server">Cooking</a>
                        <a href="~/LearningResourcePg/BrowseCourse.aspx" runat="server">Code</a>
                    </p>

                    <div class="homebuttons">
                        <a href="~/LearningResourcePg/BrowseCourse.aspx" runat="server" class="homebutton">Browse Courses</a>
                        <a href="~/LearningResourcePg/MyLearningPage.aspx" runat="server" class="homebuttonoutline">My Learning</a>
                    </div>
                </div>

                <div class="homeheroimage">
                    <asp:Image ID="imgHero" runat="server" ImageUrl="~/CommonPage/logo.png" CssClass="homeherophoto" />
                </div>

            </div>

            <!-- HOW EDUNEXUS WORKS -->
            <div class="homesection">
                <div class="homesectionhead">
                    <p class="homesectionlabel">SIMPLE JOURNEY</p>
                    <h2 class="homesectiontitle">How EduNexus Works</h2>
                    <p class="homesectiontext">Three simple steps to start learning something new.</p>
                </div>

                <div class="homecards3">

                    <div class="homecard">
                        <p class="homestepnumber">01</p>
                        <h3 class="homecardtitle">Browse Courses</h3>
                        <p class="homecardtext">Explore courses in design, music, cooking, code and more, and find the one that fits you.</p>
                        <a href="~/LearningResourcePg/BrowseCourse.aspx" runat="server" class="homecardlink">Browse now &rarr;</a>
                    </div>

                    <div class="homecard">
                        <p class="homestepnumber">02</p>
                        <h3 class="homecardtitle">Enroll and Learn</h3>
                        <p class="homecardtext">Join a course and learn at your own pace, anytime and anywhere.</p>
                        <a href="~/LearningResourcePg/BrowseCourse.aspx" runat="server" class="homecardlink">Find a course &rarr;</a>
                    </div>

                    <div class="homecard">
                        <p class="homestepnumber">03</p>
                        <h3 class="homecardtitle">Track Your Progress</h3>
                        <p class="homecardtext">Find all your courses in My Learning and pick up right where you left off.</p>
                        <a href="~/LearningResourcePg/MyLearningPage.aspx" runat="server" class="homecardlink">Go to My Learning &rarr;</a>
                    </div>

                </div>
            </div>

            <!--  POPULAR CATEGORIES -->
            <div class="homesection">
                <div class="homecategorybox">

                    <p class="homesectionlabel">EXPLORE</p>
                    <h2 class="homesectiontitle">Find something you love</h2>
                    <p class="homesectiontext">From creative skills to everyday life, there's a course for every curious learner.</p>

                    <div class="homecategorylist">
                        <span class="homecategory">🎨 Design</span>
                        <span class="homecategory">🎵 Music</span>
                        <span class="homecategory">🍳 Cooking</span>
                        <span class="homecategory">💻 Code</span>
                        <span class="homecategory">📷 Photography</span>
                        <span class="homecategory">💼 Business</span>
                    </div>

                    <a href="~/LearningResourcePg/BrowseCourse.aspx" runat="server" class="homebutton">View all courses</a>

                </div>
            </div>

            <!--  WHY EDUNEXUS  -->
            <div class="homewhy">

                <div class="homewhyleft">
                    <p class="homesectionlabel">WHY EDUNEXUS</p>
                    <h2 class="homesectiontitle">Learning that fits your life</h2>
                    <ul class="homereasons">
                        <li>Learn at your own pace, anytime and anywhere</li>
                        <li>Courses taught by experienced instructors</li>
                        <li>Keep everything in one place with My Learning</li>
                    </ul>
                </div>

                <div class="homecta">
                    <h2 class="homectatitle">Ready to start learning?</h2>
                    <p id="LoggedUserMessage" runat="server">Create your account in a minute and join your first course today.</p>
                    <a id="getStartedLink" href="~/CommonPage/RegisterPage.aspx" runat="server" class="homectabutton">Get Started</a>
                </div>

            </div>

        </div>
    </div>

</asp:Content>