<%@ Page Title="Manage Discussions" Language="C#" MasterPageFile="~/MasterPages/AdminHeader_Footer.Master" AutoEventWireup="true" CodeBehind="AdminManageDiscussion.aspx.cs" Inherits="WAPP_EduNexus_App.Admin.AdminManageDiscussion" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="AdminManageDiscussion.css?v=2" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder2" runat="server">
    <div class="admin-discussions" id="adminDiscussions">
        <section class="admin-discussions-heading">
            <div>
                <p class="admin-discussions-eyebrow">COMMUNITY MODERATION</p>
                <h1>Manage Discussions</h1>
                <p>Review discussions from every course, reply to questions, and remove posts when needed.</p>
            </div>
        </section>
        <asp:Literal ID="ltAdminNotice" runat="server" EnableViewState="false" />

        <section class="admin-discussion-stats" aria-label="Discussion summary">
            <article><span>TOTAL DISCUSSIONS</span><strong id="adminDiscussionCount">0</strong></article>
            <article><span>OPEN</span><strong id="adminOpenCount">0</strong></article>
            <article><span>ANSWERED</span><strong id="adminAnsweredCount">0</strong></article>
        </section>

        <section class="admin-discussion-toolbar" aria-label="Search and filter discussions">
            <label class="admin-discussion-search">
                <span class="visually-hidden">Search discussions</span>
                <input type="search" id="adminDiscussionSearch" placeholder="Search by topic, member, or course..." />
            </label>
            <label class="visually-hidden" for="adminCourseFilter">Filter by course</label>
            <asp:DropDownList ID="adminCourseFilter" runat="server" ClientIDMode="Static" />
            <label class="visually-hidden" for="adminDiscussionFilter">Filter by status</label>
            <select id="adminDiscussionFilter">
                <option value="all">All statuses</option>
                <option value="open">Open</option>
                <option value="answered">Answered</option>
            </select>
        </section>

        <section class="admin-discussion-table-wrap">
            <div class="admin-discussion-table-caption"><span>All course discussions</span><span id="adminShowingCount">Showing 0 discussions</span></div>
            <div class="admin-discussion-table-scroll">
                <table class="admin-discussion-table">
                    <thead>
                        <tr>
                            <th scope="col">TOPIC</th>
                            <th scope="col">COURSE</th>
                            <th scope="col">MEMBER</th>
                            <th scope="col">POSTED</th>
                            <th scope="col">REPLIES</th>
                            <th scope="col">STATUS</th>
                            <th scope="col">ACTIONS</th>
                        </tr>
                    </thead>
                    <tbody id="adminDiscussionRows">
                        <asp:Repeater ID="rptAdminDiscussionRows" runat="server" OnItemCommand="rptAdminDiscussionRows_ItemCommand">
                            <ItemTemplate>
                                <tr data-discussion-id='<%# Eval("DiscussionID") %>'
                                    data-title='<%# EncodeAttribute(Eval("Title")) %>'
                                    data-course-id='<%# Eval("CourseID") %>'
                                    data-course='<%# EncodeAttribute(Eval("CourseTitle")) %>'
                                    data-member='<%# EncodeAttribute(Eval("Username")) %>'
                                    data-status='<%# Convert.ToString(Eval("Status")).ToLowerInvariant() %>'>
                                    <td><asp:LinkButton ID="openAdminDiscussion" runat="server" CssClass="admin-topic-button" CommandName="Open" CommandArgument='<%# Eval("DiscussionID") %>' CausesValidation="false"><%#: Eval("Title") %></asp:LinkButton></td>
                                    <td><%#: Eval("CourseTitle") %></td>
                                    <td><%#: Eval("Username") %></td>
                                    <td><%# FormatMalaysiaTime(Eval("CreatedAt"), "dd MMM yyyy") %></td>
                                    <td><%# Eval("ReplyCount") %></td>
                                    <td><span class='admin-status status-<%# Convert.ToString(Eval("Status")).ToLowerInvariant() %>'><%#: Eval("Status") %></span></td>
                                    <td><div class="admin-row-actions">
                                        <asp:LinkButton ID="viewAdminDiscussion" runat="server" CssClass="admin-action-link" CommandName="Open" CommandArgument='<%# Eval("DiscussionID") %>' CausesValidation="false">View &amp; reply</asp:LinkButton>
                                        <asp:LinkButton ID="deleteAdminDiscussion" runat="server" CssClass="admin-action-link admin-delete-link" CommandName="Delete" CommandArgument='<%# Eval("DiscussionID") %>' CausesValidation="false" OnClientClick="return confirm('Delete this discussion and its replies?');">Delete</asp:LinkButton>
                                    </div></td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>
        </section>
    </div>

    <asp:Panel ID="adminDiscussionModal" runat="server" CssClass="admin-discussion-modal-backdrop" Visible="false">
        <section class="admin-discussion-modal" role="dialog" aria-modal="true" aria-labelledby="adminModalTitle">
            <a class="admin-modal-close" href="<%= ResolveUrl("~/Admin/AdminManageDiscussion.aspx") %>" aria-label="Close">&times;</a>
            <p class="admin-discussions-eyebrow">DISCUSSION REVIEW</p>
            <h2 id="adminModalTitle">Discussion details</h2>
            <div id="adminDiscussionForm">
                <dl class="admin-discussion-details">
                    <div><dt>Topic</dt><dd><asp:Literal ID="ltAdminDiscussionTitle" runat="server" /></dd></div>
                    <div><dt>Started by</dt><dd><asp:Literal ID="ltAdminDiscussionMember" runat="server" /></dd></div>
                    <div><dt>Course</dt><dd><asp:Literal ID="ltAdminDiscussionCourse" runat="server" /></dd></div>
                    <div><dt>Details</dt><dd><asp:Literal ID="ltAdminDiscussionDetails" runat="server" /></dd></div>
                    <div><dt>Status</dt><dd><span id="adminStatusBadge" runat="server" class="admin-status"><asp:Literal ID="ltAdminDiscussionStatus" runat="server" /></span></dd></div>
                </dl>
                <section class="admin-thread-replies">
                    <h3>Replies</h3>
                    <asp:Panel ID="adminEmptyReplies" runat="server" CssClass="admin-empty-replies">No replies yet.</asp:Panel>
                    <asp:Repeater ID="rptAdminReplies" runat="server">
                        <ItemTemplate>
                            <article class="admin-thread-reply">
                                <div><strong><%#: Eval("Username") %></strong><span><%#: ReplyRole(Eval("Role")) %></span></div>
                                <p><%#: Eval("ReplyText") %></p>
                                <time><%# FormatMalaysiaTime(Eval("CreatedAt"), "dd MMM yyyy, HH:mm") %></time>
                            </article>
                        </ItemTemplate>
                    </asp:Repeater>
                </section>
                <label for="adminReplyInput">Add an admin reply</label>
                <asp:TextBox ID="adminReplyInput" runat="server" ClientIDMode="Static" TextMode="MultiLine" Rows="3" MaxLength="2000" placeholder="Write a helpful reply..." />
                <div class="admin-modal-actions">
                    <a class="admin-discussion-secondary" href="<%= ResolveUrl("~/Admin/AdminManageDiscussion.aspx") %>">Back to discussions</a>
                    <asp:Button ID="saveAdminReply" runat="server" CssClass="admin-discussion-primary" Text="Post reply" OnClick="saveAdminReply_Click" CausesValidation="false" formnovalidate="formnovalidate" />
                </div>
            </div>
        </section>
    </asp:Panel>

    <script>
        (function () {
            var rows = document.getElementById('adminDiscussionRows');

            function updateSummary() {
                var allRows = Array.prototype.slice.call(rows.querySelectorAll('tr'));
                var visibleRows = allRows.filter(function (row) { return !row.hidden; });
                document.getElementById('adminDiscussionCount').textContent = allRows.length;
                document.getElementById('adminOpenCount').textContent = allRows.filter(function (row) { return row.dataset.status === 'open'; }).length;
                document.getElementById('adminAnsweredCount').textContent = allRows.filter(function (row) { return row.dataset.status === 'answered'; }).length;
                document.getElementById('adminShowingCount').textContent = 'Showing ' + visibleRows.length + ' discussion' + (visibleRows.length === 1 ? '' : 's');
            }

            function applyFilters() {
                var query = document.getElementById('adminDiscussionSearch').value.trim().toLowerCase();
                var course = document.getElementById('adminCourseFilter').value;
                var status = document.getElementById('adminDiscussionFilter').value;
                Array.prototype.forEach.call(rows.querySelectorAll('tr'), function (row) {
                    var text = (row.dataset.title + ' ' + row.dataset.member + ' ' + row.dataset.course).toLowerCase();
                    row.hidden = (query && text.indexOf(query) === -1) ||
                        (course !== 'all' && row.dataset.courseId !== course) ||
                        (status !== 'all' && row.dataset.status !== status);
                });
                updateSummary();
            }

            document.getElementById('adminDiscussionSearch').addEventListener('input', applyFilters);
            document.getElementById('adminCourseFilter').addEventListener('change', applyFilters);
            document.getElementById('adminDiscussionFilter').addEventListener('change', applyFilters);
            updateSummary();
        }());
    </script>
</asp:Content>
