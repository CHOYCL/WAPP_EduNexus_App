<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/AdminHeader_Footer.Master" AutoEventWireup="true" CodeBehind="AdminManageCourseDetail.aspx.cs" Inherits="WAPP_EduNexus_App.AdminManageCourseDetail" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder2" runat="server">
    <link href="AdminManageCourseDetail.css" rel="stylesheet" />
    <div class="page-container">
        <section class="Section1">
            <div class="contentIMG_VID">
                <div class="media-placeholder">
                    <asp:Image ID="imgCourseMedia" runat="server" CssClass="courseMedia" Visible="false" />

                    <video class="courseMedia" id="courseVID" runat="server" controls style="display:none;">
                        <source id="videoSource" runat="server" src="#" type="video/mp4" />
                        Your browser does not support the video tag.
                    </video>
                </div>

                <div class="upload-controls">
                    <asp:FileUpload ID="fuMedia" runat="server" onchange="previewSelectedMedia(this)" />
                    <asp:Button ID="btnUploadMedia" runat="server" Text="Upload File/Video" OnClick="btnUploadMedia_Click" />
                </div>
            </div>
            <div class ="content-description">
                <p class="titleNaming">COURSE TITLE</p>
                <div style="display:flex; align-items:center; justify-content:space-between;">
                    <h1 class="course-title"><asp:Literal ID="ltCourseTitle" runat="server" /></h1>
                    <div>
                        <asp:Button ID="btnEditTitle" runat="server" CssClass="small-button" Text="Edit" OnClick="btnEditTitle_Click" />
                    </div>
                </div>
                <!-- Hidden field retained for compatibility; server-side edit UI appears in the panel below -->
                <asp:HiddenField ID="hfNewCourseTitle" runat="server" />
                <asp:Button ID="btnSaveTitle" runat="server" Text="Save" OnClick="btnSaveTitle_Click" Style="display:none;" />
                <asp:Panel ID="pnlEditTitle" runat="server" Visible="false">
                    <asp:TextBox ID="txtTitleEdit" runat="server" CssClass="form-input" />
                    <div style="margin-top:8px; display:flex; gap:8px; align-items:center;">
                        <label style="font-weight:600; margin-right:4px;">Category</label>
                        <asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-input">
                            <asp:ListItem Value="">-- Select --</asp:ListItem>
                            <asp:ListItem Value="General">General</asp:ListItem>
                            <asp:ListItem Value="Programming">Programming</asp:ListItem>
                            <asp:ListItem Value="Math">Math</asp:ListItem>
                            <asp:ListItem Value="Science">Science</asp:ListItem>
                            <asp:ListItem Value="Language">Language</asp:ListItem>
                        </asp:DropDownList>

                        <label style="font-weight:600; margin-right:4px;">Level</label>
                        <asp:DropDownList ID="ddlLevel" runat="server" CssClass="form-input">
                            <asp:ListItem Value="">-- Select --</asp:ListItem>
                            <asp:ListItem Value="Beginner">Beginner</asp:ListItem>
                            <asp:ListItem Value="Intermediate">Intermediate</asp:ListItem>
                            <asp:ListItem Value="Advanced">Advanced</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div style="margin-top:8px;">
                        <label style="font-weight:600; display:block; margin-bottom:4px;">Instructor</label>
                        <asp:TextBox ID="txtInstructorEdit" runat="server" CssClass="form-input" />
                    </div>
                    <div style="margin-top:8px;">
                        <label style="font-weight:600; display:block; margin-bottom:4px;">Price</label>
                        <asp:TextBox ID="txtPriceEdit" runat="server" CssClass="form-input" />
                    </div>
                    <asp:Button ID="btnSaveTitlePanel" runat="server" Text="Save" OnClick="btnSaveTitle_Click" Style="margin-top:8px;" />
                </asp:Panel>
                <p class="titleNaming">DESCRIPTION</p>
                <div style="display:flex; align-items:center; justify-content:space-between;">
                    <p class="course-desc"><asp:Literal ID="ltMaterialDescription" runat="server" /></p>
                    <asp:Button ID="btnEditDescription" runat="server" CssClass="small-button" Text="Edit" OnClick="btnEditDescription_Click" />
                </div>
                <asp:Panel ID="pnlEditDescription" runat="server" Visible="false">
                    <asp:TextBox ID="txtDescriptionEdit" runat="server" TextMode="MultiLine" Rows="4" CssClass="form-input" />
                    <asp:Button ID="btnSaveDescription" runat="server" Text="Save" OnClick="btnSaveDescription_Click" />
                </asp:Panel>

                <div class="tag-row">
                    <asp:Literal ID="ltCategory" runat="server" />
                    <asp:Literal ID="ltLevel" runat="server" />
                </div>
                <div class="tag-row">
                    <asp:Literal ID="ltInstructor" runat="server" />
                </div>
            </div>
        </section>
    </div>
    <script type="text/javascript">
        // @ts-nocheck
        /** @param {HTMLInputElement} input */
        function previewSelectedMedia(input) {
            if (!input || !input.files || !input.files[0]) return;
            var file = input.files[0];
            var ext = file.name.split('.').pop().toLowerCase();
            var img = document.getElementById('<%= imgCourseMedia.ClientID %>');
            var video = document.getElementById('<%= courseVID.ClientID %>');
            var videoSource = document.getElementById('<%= videoSource.ClientID %>');
            var imageExts = ['png', 'jpg', 'jpeg', 'gif', 'bmp', 'webp'];
            var videoExts = ['mp4', 'webm', 'ogg', 'mov'];
            if (imageExts.indexOf(ext) !== -1) {
                var reader = new FileReader();
                reader.onload = function (e) {
                    if (img && e && e.target) {
                        /** @type {HTMLImageElement} */ (img).src = /** @type {any} */ (e.target.result);
                        img.style.display = '';
                    }
                    if (video) video.style.display = 'none';
                    if (videoSource && videoSource.src !== undefined) videoSource.src = '';
                };
                reader.readAsDataURL(file);
            } else if (videoExts.indexOf(ext) !== -1) {
                var url = URL.createObjectURL(file);
                if (videoSource && videoSource.src !== undefined) videoSource.src = url;
                if (video && video.load) {
                    video.load();
                    video.style.display = '';
                }
                if (img) img.style.display = 'none';
            } else {
                if (img) img.style.display = 'none';
                if (video) video.style.display = 'none';
            }
        }
    </script>
</asp:Content>
