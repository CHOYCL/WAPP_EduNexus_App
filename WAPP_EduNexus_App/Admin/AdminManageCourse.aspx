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
                <button type="button" onclick="openForm()">Create New Course</button>
            </div>
        </section>

        <section class="Section2">
            <div class="total-course-card">
                <p>TOTAL COURSE</p>
                <p></p>
                <p></p>
            </div>
            <div class="total-course-active-card">
                <p>ACTIVE ENROLLEES</p>
                <p></p>
                <p></p>
            </div>
            <div class="total-course-complete-card">
                <p>COMPLETED COURSES</p>
                <p></p>
                <p></p>
            </div>
        </section>
        <section class="Section3">
            <div class="search-container">
                <div class="search-box">
                    <svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" aria-hidden="true" role="img" width="25" height="25" viewBox="0 0 24 24" style="color: rgb(74, 85, 101); opacity: 1; transform: rotate(0deg);"><g fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2"><path d="m21 21l-4.34-4.34"></path><circle cx="11" cy="11" r="8"></circle></g></svg>
                    <asp:TextBox ID="txtSearch" runat="server" CssClass="search-input" placeholder="Search for courses..." />
                </div>

                <div class="dropdown-container">
                    <asp:DropDownList ID="ddlCategory" runat="server" CssClass="filter-dropdown">
                    <asp:ListItem Text="All categories" Value="" />
                    <asp:ListItem Text="Programming" Value="Programming" />
                    <asp:ListItem Text="Business" Value="Photography" />
                    <asp:ListItem Text="Design" Value="Design" />
                </asp:DropDownList>
                    <span class="dropdown-icon"><svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" aria-hidden="true" role="img" width="30" height="30" viewBox="0 0 24 24" style="color: rgb(74, 85, 101); opacity: 1; transform: rotate(0deg);"><path fill="currentColor" d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10s10-4.48 10-10S17.52 2 12 2m-.35 12.65l-2.79-2.79c-.32-.32-.1-.86.35-.86h5.59c.45 0 .67.54.35.85l-2.79 2.79c-.2.2-.52.2-.71.01"></path></svg></span>
                </div>

                <div class="dropdown-container">
                    <asp:DropDownList ID="ddlLevel" runat="server" CssClass="filter-dropdown">
                    <asp:ListItem Text="Any level" Value="" />
                    <asp:ListItem Text="Beginner" Value="Beginner" />
                    <asp:ListItem Text="Intermediate" Value="Intermediate" />
                    <asp:ListItem Text="Advanced" Value="Advanced" />
                </asp:DropDownList>
                    <span class="dropdown-icon"><svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" aria-hidden="true" role="img" width="30" height="30" viewBox="0 0 24 24" style="color: rgb(74, 85, 101); opacity: 1; transform: rotate(0deg);"><path fill="currentColor" d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10s10-4.48 10-10S17.52 2 12 2m-.35 12.65l-2.79-2.79c-.32-.32-.1-.86.35-.86h5.59c.45 0 .67.54.35.85l-2.79 2.79c-.2.2-.52.2-.71.01"></path></svg></span>
                </div>

                <asp:Button ID="btnApply" runat="server" Text="Apply" CssClass="apply-button" OnClick="btnApply_Click" />
            </div>
        </section>

        <section class="Section4">
            <div class="course-list-container">
                <asp:Literal ID="ltCoursesMessage" runat="server" />
                <asp:Literal ID="ltCoursesCount" runat="server" />
                <!-- TABLE HEADER -->
                <div class="course-row course-header">
                    <div class="select-cell">
                        <input type="checkbox" id="chkSelectAll" aria-label="Select all courses" onchange="toggleSelectAll(this)" />
                    </div>
                    <div>COURSE &amp; CURRICULUM</div>
                    <div>INSTRUCTOR</div>
                    <div>CATEGORY</div>
                    <div>LEVEL</div>
                    <div>PRICE</div>
                    <div>STUDENTS</div>
                </div>

                <asp:Repeater ID="rptAdminCourses" runat="server" OnItemCommand="rptAdminCourses_ItemCommand">
                    <ItemTemplate>
                        <div class="course-row">
                            <div class="select-cell">
                                <input type="checkbox" class="select-course-checkbox" aria-label='<%# Eval("courseTitle") %>' onchange="onCourseCheckboxChange(this)" />
                            </div>

                            <div class="course-info">
                                <div class="course-thumbnail">
                                    <%# string.IsNullOrEmpty(Convert.ToString(Eval("Thumbnail"))) ? "<img src='/assets/images/default-course.png' alt='' />" : ("<img src='" + ResolveUrl("~/" + Convert.ToString(Eval("Thumbnail")).TrimStart('/') ) + "' alt='' />") %>
                                </div>
                                <div class="course-details">
                                    <h3><%# Eval("courseTitle") %></h3>
                                    <p><%# Eval("MetaInfo") %></p>
                                </div>
                            </div>

                            <div class="instructor">
                                <div class="instructor-avatar"><%# Eval("InstructorInitials") %></div>
                                <span><%# Eval("InstructorName") %></span>
                            </div>

                            <div><span class="category-tag"><%# Eval("Category") %></span></div>
                            <div><span class="level-tag"><%# Eval("level") %></span></div>
                            <div class="course-price"><%# Eval("Price", "{0:C}") %></div>

                            <div class="course-rating">
                                <p><%# Eval("Students") %> students</p>
                            </div>

                            <div class="course-actions" style="display:none;">
                                <asp:Button runat="server" CssClass="action-button" Text="Edit" CommandName="EditCourse" CommandArgument='<%# Eval("courseID") %>' />
                                <asp:Button runat="server" CssClass="action-button1" Text="Delete" CommandName="DeleteCourse" CommandArgument='<%# Eval("courseID") %>' OnClientClick="return confirm('Delete this course?');" />
                            </div>
                        </div>
                    </ItemTemplate>
                    <FooterTemplate>
                        <!-- PAGINATION can be rendered here or separately -->
                        <div class="course-pagination">
                            <span>Showing <strong>1 to 4</strong> of <strong>184</strong> courses</span>

                            <div class="page-size">
                                <label for="pageSize">Page:</label>
                                <select id="pageSize">
                                    <option>10</option>
                                    <option>20</option>
                                    <option>50</option>
                                </select>
                            </div>

                            <div class="page-buttons">
                                <button type="button" disabled>‹</button>
                                <button type="button" class="current-page">1</button>
                                <button type="button">2</button>
                                <button type="button">3</button>
                                <span>...</span>
                                <button type="button">31</button>
                                <button type="button">›</button>
                            </div>
                        </div>
                    </FooterTemplate>
                </asp:Repeater>

            </div>
        </section>
    </div>







    <%--POP OUT FORM--%>
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
            <label for="<%= ddlCourseLevel.ClientID %>">Level</label>
            <asp:DropDownList ID="ddlCourseLevel" runat="server" CssClass="form-input">
                <asp:ListItem Text="Select level" Value="" />
                <asp:ListItem Text="Beginner" Value="Beginner" />
                <asp:ListItem Text="Intermediate" Value="Intermediate" />
                <asp:ListItem Text="Advanced" Value="Advanced" />
            </asp:DropDownList>
        </div>

        <div class="form-group">
            <label for="<%= txtPrice.ClientID %>">Price</label>
            <asp:TextBox ID="txtPrice" runat="server" CssClass="form-input" placeholder="Enter price (e.g. 19.99)" />
        </div>

        <asp:FileUpload ID="fuImage" runat="server" CssClass="form-input" accept="image/*" onchange="previewFile(this)" />
        <asp:Image ID="imgPreview" runat="server" CssClass="image-preview" Visible="false" />
        <asp:HiddenField ID="hfEditingCourseId" runat="server" />

        <asp:Button ID="btnSaveCourse" runat="server"
            Text="Save Course"
            CssClass="save-button"
            OnClick="btnSaveCourse_Click" />

        </div>
    </div>

    <script type="text/javascript">
        function openForm() {
            document.getElementById('courseModal').style.display = 'flex';
        }

        function closeForm() {
            document.getElementById('courseModal').style.display = 'none';
        }

        // Close modal when clicking outside the form
        window.addEventListener('click', function (event) {
            var modal = document.getElementById('courseModal');
            if (event.target === modal) {
                closeForm();
            }
        });

        // Close modal when pressing Escape
        document.addEventListener('keydown', function (event) {
            if (event.key === 'Escape') {
                closeForm();
            }
        });
    </script>
    <script type="text/javascript">
        function onCourseCheckboxChange(chk) {
            // Find the parent course-row element
            var node = chk;
            while (node && !node.classList.contains('course-row')) node = node.parentNode;
            if (!node) return;
            var actions = node.querySelector('.course-actions');
            if (actions) actions.style.display = chk.checked ? 'flex' : 'none';
        }

        function toggleSelectAll(master) {
            var boxes = document.querySelectorAll('.select-course-checkbox');
            boxes.forEach(function (b) {
                b.checked = master.checked;
                // trigger change handler to show/hide actions
                onCourseCheckboxChange(b);
            });
        }

        // Preview selected image file in the modal before upload
        function previewFile(input) {
            if (!input || !input.files || !input.files[0]) return;
            var file = input.files[0];
            var allowed = ['image/png', 'image/jpeg', 'image/jpg', 'image/gif'];
            if (allowed.indexOf(file.type) === -1) {
                alert('Please select a PNG, JPG or GIF image.');
                input.value = '';
                return;
            }
            var reader = new FileReader();
            reader.onload = function (e) {
                var img = document.getElementById('<%= imgPreview.ClientID %>');
                if (img) {
                    img.src = e.target.result;
                    img.style.display = 'block';
                }
            };
            reader.readAsDataURL(file);
        }
    </script>
</asp:Content>
