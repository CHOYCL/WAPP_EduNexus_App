
<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/AdminHeader_Footer.Master" AutoEventWireup="true" CodeBehind="AdminManageCourse.aspx.cs" Inherits="WAPP_EduNexus_App.AdminManageCourse" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder2" runat="server">

    <link href="AdminManageCourse.css" rel="stylesheet" />

    <div class="page-container">

        <section class="Section1">
            <div class="TitleSection">
                <h1 class="page-title">Course Management</h1>
                <p>Manage catalog curriculum status, instructor assignments,<br /> pricing, and enrollments.</p>

                <asp:Button ID="btnCreateCourse" runat="server"
                    Text="Create New Course"
                    CssClass="create-button"
                    OnClick="btnCreateCourse_Click" />
            </div>
        </section>

        <!-- DASHBOARD STATISTICS -->
        <section class="Section2">

            <div class="total-course-card">
                <p>TOTAL COURSE</p>
                <p class="total-course-number">
                    <asp:Literal ID="ltTotalCourseCount" runat="server" Text="0" />
                </p>
            </div>

            <div class="total-course-active-card">
                <p>ACTIVE ENROLLEES</p>
                <p class="total-course-number">
                    <asp:Literal ID="ltActiveEnrolleesCount" runat="server" Text="0" />
                </p>
            </div>

            <div class="total-course-complete-card">
                <p>COMPLETED COURSES</p>
                <p class="total-course-number">
                    <asp:Literal ID="ltCompletedCoursesCount" runat="server" Text="0" />
                </p>
            </div>

        </section>

        <!-- SEARCH AND FILTER -->
        <section class="Section3">

            <div class="search-container">

                <div class="search-box">
                    <svg xmlns="http://www.w3.org/2000/svg"
                        aria-hidden="true" role="img"
                        width="25" height="25" viewBox="0 0 24 24"
                        style="color: rgb(74, 85, 101);">
                        <g fill="none" stroke="currentColor"
                            stroke-linecap="round" stroke-linejoin="round"
                            stroke-width="2">
                            <path d="m21 21l-4.34-4.34"></path>
                            <circle cx="11" cy="11" r="8"></circle>
                        </g>
                    </svg>

                    <asp:TextBox ID="txtSearch" runat="server"
                        CssClass="search-input"
                        placeholder="Search for courses..." />
                </div>

                <div class="dropdown-container">

                    <asp:DropDownList ID="ddlCategory" runat="server"
                        CssClass="filter-dropdown">

                        <asp:ListItem Text="All categories" Value="" />
                        <asp:ListItem Text="Programming" Value="Programming" />
                        <asp:ListItem Text="Business" Value="Photography" />
                        <asp:ListItem Text="Design" Value="Design" />

                    </asp:DropDownList>

                    <span class="dropdown-icon">▼</span>
                </div>

                <div class="dropdown-container">

                    <asp:DropDownList ID="ddlLevel" runat="server"
                        CssClass="filter-dropdown">

                        <asp:ListItem Text="Any level" Value="" />
                        <asp:ListItem Text="Beginner" Value="Beginner" />
                        <asp:ListItem Text="Intermediate" Value="Intermediate" />
                        <asp:ListItem Text="Advanced" Value="Advanced" />

                    </asp:DropDownList>

                    <span class="dropdown-icon">▼</span>
                </div>

                <asp:Button ID="btnApply" runat="server"
                    Text="Apply"
                    CssClass="apply-button"
                    OnClick="btnApply_Click" />

            </div>

        </section>

        <!-- COURSE TABLE -->
        <section class="Section4">

            <div class="course-list-container">

                <asp:Literal ID="ltCoursesMessage" runat="server" />
                <asp:Literal ID="ltCoursesCount" runat="server" />

                <!-- TABLE HEADER -->
                <div class="course-row course-header">

                    <div class="select-cell">
                        <input type="checkbox"
                            id="chkSelectAll"
                            aria-label="Select all courses"
                            onchange="toggleSelectAll(this)" />
                    </div>

                    <div>COURSE &amp; CURRICULUM</div>
                    <div>INSTRUCTOR</div>
                    <div>CATEGORY</div>
                    <div>LEVEL</div>
                    <div>PRICE</div>
                    <div>STUDENTS</div>

                </div>

                <!-- COURSE REPEATER -->
                <asp:Repeater ID="rptAdminCourses" runat="server"
                    OnItemCommand="rptAdminCourses_ItemCommand">

                    <ItemTemplate>

                        <div class="course-row">

                            <div class="select-cell">
                                <input type="checkbox"
                                    class="select-course-checkbox"
                                    aria-label='<%# Eval("courseTitle") %>'
                                    onchange="onCourseCheckboxChange(this)" />
                            </div>

                            <div class="course-info">
                                <div class="course-details">
                                    <h3><%# Eval("courseTitle") %></h3>
                                    <p><%# Eval("MetaInfo") %></p>
                                </div>
                            </div>

                            <div class="instructor">
                                <div class="instructor-avatar">
                                    <%# Eval("InstructorInitials") %>
                                </div>
                                <span><%# Eval("InstructorName") %></span>
                            </div>

                            <div class="course-category">
                                <span class="category-tag">
                                    <%# string.IsNullOrWhiteSpace(Convert.ToString(Eval("Category")))
                                        ? "Not set"
                                        : Server.HtmlEncode(Convert.ToString(Eval("Category"))) %>
                                </span>
                            </div>

                            <div class="course-level">
                                <span class="level-tag">
                                    <%# string.IsNullOrWhiteSpace(Convert.ToString(Eval("level")))
                                        ? "Not set"
                                        : Server.HtmlEncode(Convert.ToString(Eval("level"))) %>
                                </span>
                            </div>

                            <div class="course-price">
                                <%# Eval("Price", "{0:C}") %>
                            </div>

                            <div class="course-rating">
                                <p><%# Eval("Students") %> students</p>
                            </div>

                            <div class="course-actions" style="display:none;">

                                <asp:Button runat="server"
                                    CssClass="action-button"
                                    Text="Edit"
                                    CommandName="EditCourse"
                                    CommandArgument='<%# Eval("courseID") %>' />

                                <asp:Button runat="server"
                                    CssClass="action-button"
                                    Text="Modules"
                                    CommandName="ManageModules"
                                    CommandArgument='<%# Eval("courseID") %>' />

                                <asp:Button runat="server"
                                    CssClass="action-button1"
                                    Text="Delete"
                                    CommandName="DeleteCourse"
                                    CommandArgument='<%# Eval("courseID") %>'
                                    OnClientClick="return confirm('Delete this course?');" />

                            </div>

                        </div>

                    </ItemTemplate>

                </asp:Repeater>

                <!-- FUNCTIONAL PAGINATION -->
                <!-- IMPORTANT: This panel is OUTSIDE rptAdminCourses -->

                <asp:Panel ID="pnlPagination" runat="server"
                    CssClass="course-pagination">

                    <div class="page-size">

                        <span>Rows per page:</span>

                        <asp:DropDownList ID="ddlPageSize" runat="server"
                            CssClass="page-size-dropdown"
                            AutoPostBack="true"
                            OnSelectedIndexChanged="ddlPageSize_SelectedIndexChanged">

                            <asp:ListItem Text="10" Value="10" />
                            <asp:ListItem Text="20" Value="20" />
                            <asp:ListItem Text="50" Value="50" />

                        </asp:DropDownList>

                    </div>

                    <div class="page-buttons">

                        <asp:LinkButton ID="btnPreviousPage" runat="server"
                            Text="&lt;"
                            OnClick="btnPreviousPage_Click" />

                        <asp:Repeater ID="rptPagination" runat="server"
                            OnItemCommand="rptPagination_ItemCommand">

                            <ItemTemplate>

                                <asp:LinkButton ID="btnPage" runat="server"
                                    Text='<%# Eval("PageNumber") %>'
                                    CommandName="GoToPage"
                                    CommandArgument='<%# Eval("PageNumber") %>'
                                    CssClass='<%# Eval("CssClass") %>'
                                    Enabled='<%# Convert.ToBoolean(Eval("IsEnabled")) %>' />

                            </ItemTemplate>

                        </asp:Repeater>

                        <asp:LinkButton ID="btnNextPage" runat="server"
                            Text="&gt;"
                            OnClick="btnNextPage_Click" />

                    </div>

                </asp:Panel>

            </div>

        </section>

    </div>

    <!-- POP OUT FORM -->
    <div id="courseModal" class="modal">

        <div class="modal-content">

            <span class="close" onclick="closeForm()">&times;</span>

            <h2>Add New Course</h2>

            <div class="form-group">
                <label for="<%= txtCourseName.ClientID %>">
                    Course Name
                </label>

                <asp:TextBox ID="txtCourseName" runat="server"
                    CssClass="form-input"
                    placeholder="Enter course name" />
            </div>

            <div class="form-group">
                <label for="<%= txtInstructor.ClientID %>">
                    Instructor
                </label>

                <asp:TextBox ID="txtInstructor" runat="server"
                    CssClass="form-input"
                    placeholder="Enter instructor name" />
            </div>

            <div class="form-group">
                <label for="<%= txtDescription.ClientID %>">
                    Description
                </label>

                <asp:TextBox ID="txtDescription" runat="server"
                    CssClass="form-input"
                    TextMode="MultiLine"
                    Rows="4"
                    placeholder="Enter course description" />
            </div>

            <div class="form-group">
                <label for="<%= ddlCourseLevel.ClientID %>">
                    Level
                </label>

                <asp:DropDownList ID="ddlCourseLevel" runat="server"
                    CssClass="form-input">

                    <asp:ListItem Text="Select level" Value="" />
                    <asp:ListItem Text="Beginner" Value="Beginner" />
                    <asp:ListItem Text="Intermediate" Value="Intermediate" />
                    <asp:ListItem Text="Advanced" Value="Advanced" />

                </asp:DropDownList>
            </div>

            <div class="form-group">
                <label for="<%= txtPrice.ClientID %>">
                    Price
                </label>

                <asp:TextBox ID="txtPrice" runat="server"
                    CssClass="form-input"
                    placeholder="Enter price (e.g. 19.99)" />
            </div>

            <asp:FileUpload ID="fuImage" runat="server"
                CssClass="form-input"
                accept="image/*"
                onchange="previewFile(this)" />

            <asp:Image ID="imgPreview" runat="server"
                CssClass="image-preview"
                Visible="false" />

            <asp:HiddenField ID="hfEditingCourseId" runat="server" />

            <asp:Button ID="btnSaveCourse" runat="server"
                Text="Save Course"
                CssClass="save-button"
                OnClick="btnSaveCourse_Click" />

        </div>

    </div>

    <!-- MODAL JAVASCRIPT -->
    <script type="text/javascript">

        function openForm() {
            var modal = document.getElementById('courseModal');
            if (modal) modal.style.display = 'flex';
        }

        function closeForm() {
            var modal = document.getElementById('courseModal');
            if (modal) modal.style.display = 'none';
        }

        window.addEventListener('click', function (event) {
            var modal = document.getElementById('courseModal');

            if (modal && event.target === modal) {
                closeForm();
            }
        });

        document.addEventListener('keydown', function (event) {
            if (event.key === 'Escape') {
                closeForm();
            }
        });

    </script>

    <!-- COURSE SELECTION AND IMAGE PREVIEW -->
    <script type="text/javascript">

        function onCourseCheckboxChange(chk) {

            var node = chk;

            while (node && !node.classList.contains('course-row')) {
                node = node.parentNode;
            }

            if (!node) return;

            var actions = node.querySelector('.course-actions');

            if (actions) {
                actions.style.display = chk.checked ? 'flex' : 'none';
            }
        }

        function toggleSelectAll(master) {

            var boxes = document.querySelectorAll('.select-course-checkbox');

            boxes.forEach(function (b) {
                b.checked = master.checked;
                onCourseCheckboxChange(b);
            });
        }

        function previewFile(input) {

            if (!input || !input.files || !input.files[0]) return;

            var file = input.files[0];

            var allowed = [
                'image/png',
                'image/jpeg',
                'image/jpg',
                'image/gif'
            ];

            if (allowed.indexOf(file.type) === -1) {
                alert('Please select a PNG, JPG or GIF image.');
                input.value = '';
                return;
            }

            var reader = new FileReader();

            reader.onload = function (e) {

                var img = document.getElementById('<%= imgPreview.ClientID %>');

                if (img && e && e.target) {
                    img.src = e.target.result;
                    img.style.display = 'block';
                }
            };

            reader.readAsDataURL(file);
        }

    </script>

</asp:Content>
