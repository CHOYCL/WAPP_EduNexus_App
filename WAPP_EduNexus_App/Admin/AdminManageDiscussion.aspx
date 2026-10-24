<%@ Page Title="Manage Discussions" Language="C#" MasterPageFile="~/MasterPages/AdminHeader_Footer.Master" AutoEventWireup="true" CodeBehind="AdminManageDiscussion.aspx.cs" Inherits="WAPP_EduNexus_App.Admin.AdminManageDiscussion" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="AdminManageDiscussion.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder2" runat="server">
    <div class="admin-discussions" id="adminDiscussions">
        <section class="admin-discussions-heading">
            <div>
                <p class="admin-discussions-eyebrow">COMMUNITY MODERATION</p>
                <h1>Manage Discussions</h1>
                <p>Review conversations from every course and moderate posts when needed.</p>
            </div>
        </section>

        <section class="admin-discussion-stats" aria-label="Discussion summary">
            <article><span>TOTAL DISCUSSIONS</span><strong id="adminDiscussionCount">0</strong></article>
            <article><span>OPEN</span><strong id="adminOpenCount">0</strong></article>
            <article><span>HIDDEN</span><strong id="adminHiddenCount">0</strong></article>
        </section>

        <section class="admin-discussion-toolbar" aria-label="Search and filter discussions">
            <label class="admin-discussion-search">
                <span class="visually-hidden">Search discussions</span>
                <input type="search" id="adminDiscussionSearch" placeholder="Search by topic, member, or course..." />
            </label>
            <label class="visually-hidden" for="adminCourseFilter">Filter by course</label>
            <select id="adminCourseFilter">
                <option value="all">All courses</option>
                <option value="Night Photography">Night Photography</option>
                <option value="Digital Design">Digital Design</option>
                <option value="JavaScript Basics">JavaScript Basics</option>
            </select>
            <label class="visually-hidden" for="adminDiscussionFilter">Filter by status</label>
            <select id="adminDiscussionFilter">
                <option value="all">All statuses</option>
                <option value="open">Open</option>
                <option value="closed">Closed</option>
                <option value="hidden">Hidden</option>
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
                            <th scope="col">MODERATION</th>
                        </tr>
                    </thead>
                    <tbody id="adminDiscussionRows">
                    </tbody>
                </table>
            </div>
        </section>
    </div>

    <div class="admin-discussion-modal-backdrop" id="adminDiscussionModal" hidden>
        <section class="admin-discussion-modal" role="dialog" aria-modal="true" aria-labelledby="adminModalTitle">
            <button type="button" class="admin-modal-close" id="closeAdminModal" aria-label="Close">&times;</button>
            <p class="admin-discussions-eyebrow">DISCUSSION REVIEW</p>
            <h2 id="adminModalTitle">Discussion details</h2>
            <div id="adminDiscussionForm" role="form">
                <dl class="admin-discussion-details">
                    <div><dt>Topic</dt><dd id="adminDiscussionTitle"></dd></div>
                    <div><dt>Member</dt><dd id="adminDiscussionMember"></dd></div>
                    <div><dt>Course</dt><dd id="adminDiscussionCourse"></dd></div>
                    <div><dt>Details</dt><dd id="adminDiscussionBody"></dd></div>
                </dl>
                <label for="adminDiscussionStatus">Moderation status</label>
                <select id="adminDiscussionStatus">
                    <option value="open">Open</option>
                    <option value="closed">Closed</option>
                    <option value="hidden">Hidden</option>
                </select>
                <div class="admin-modal-actions">
                    <button type="button" class="admin-discussion-secondary" id="cancelAdminModal">Close</button>
                    <button type="button" class="admin-discussion-primary" id="saveAdminDiscussion">Update status</button>
                </div>
            </div>
        </section>
    </div>

    <script>
        (function () {
            var rows = document.getElementById('adminDiscussionRows');
            var modal = document.getElementById('adminDiscussionModal');
            var topicValue = document.getElementById('adminDiscussionTitle');
            var memberValue = document.getElementById('adminDiscussionMember');
            var courseValue = document.getElementById('adminDiscussionCourse');
            var bodyValue = document.getElementById('adminDiscussionBody');
            var statusInput = document.getElementById('adminDiscussionStatus');
            var editingRow = null;
            var viewingOnly = false;

            function statusLabel(value) {
                return { open: 'Open', closed: 'Closed', hidden: 'Hidden' }[value] || 'Open';
            }

            function updateSummary() {
                var allRows = Array.prototype.slice.call(rows.querySelectorAll('tr'));
                var visibleRows = allRows.filter(function (row) { return !row.hidden; });
                document.getElementById('adminDiscussionCount').textContent = allRows.length;
                document.getElementById('adminOpenCount').textContent = allRows.filter(function (row) { return row.dataset.status === 'open'; }).length;
                document.getElementById('adminHiddenCount').textContent = allRows.filter(function (row) { return row.dataset.status === 'hidden'; }).length;
                document.getElementById('adminShowingCount').textContent = 'Showing ' + visibleRows.length + ' discussion' + (visibleRows.length === 1 ? '' : 's');
            }

            function closeModal() {
                modal.hidden = true;
                statusInput.value = 'open';
                editingRow = null;
                viewingOnly = false;
                statusInput.disabled = false;
                document.getElementById('saveAdminDiscussion').hidden = false;
            }

            function openModal(row, mode) {
                editingRow = row;
                viewingOnly = mode === 'view';
                topicValue.textContent = row.dataset.title;
                memberValue.textContent = row.dataset.member;
                courseValue.textContent = row.dataset.course;
                bodyValue.textContent = row.dataset.body;
                statusInput.value = row.dataset.status;
                statusInput.disabled = viewingOnly;
                document.getElementById('saveAdminDiscussion').hidden = viewingOnly;
                document.getElementById('adminModalTitle').textContent = mode === 'view' ? 'Discussion details' : 'Update moderation status';
                modal.hidden = false;
            }

            function applyFilters() {
                var query = document.getElementById('adminDiscussionSearch').value.trim().toLowerCase();
                var course = document.getElementById('adminCourseFilter').value;
                var status = document.getElementById('adminDiscussionFilter').value;
                Array.prototype.forEach.call(rows.querySelectorAll('tr'), function (row) {
                    var text = (row.dataset.title + ' ' + row.dataset.member + ' ' + row.dataset.course).toLowerCase();
                    row.hidden = (query && text.indexOf(query) === -1) ||
                        (course !== 'all' && row.dataset.course !== course) ||
                        (status !== 'all' && row.dataset.status !== status);
                });
                updateSummary();
            }

            document.getElementById('closeAdminModal').addEventListener('click', closeModal);
            document.getElementById('cancelAdminModal').addEventListener('click', closeModal);
            modal.addEventListener('click', function (event) { if (event.target === modal) closeModal(); });
            document.getElementById('adminDiscussionSearch').addEventListener('input', applyFilters);
            document.getElementById('adminCourseFilter').addEventListener('change', applyFilters);
            document.getElementById('adminDiscussionFilter').addEventListener('change', applyFilters);

            rows.addEventListener('click', function (event) {
                var button = event.target.closest('button[data-action]');
                if (!button) return;
                var row = button.closest('tr');
                if (button.dataset.action === 'delete') {
                    if (window.confirm('Delete this discussion?')) {
                        row.remove();
                        applyFilters();
                    }
                } else {
                    openModal(row, button.dataset.action === 'edit' ? 'edit' : 'view');
                }
            });

            document.getElementById('saveAdminDiscussion').addEventListener('click', function () {
                if (viewingOnly || !editingRow) return;
                var status = statusInput.value;
                editingRow.dataset.status = status;
                var badge = editingRow.querySelector('.admin-status');
                badge.className = 'admin-status status-' + status;
                badge.textContent = statusLabel(status);
                closeModal();
                applyFilters();
            });

            updateSummary();
        }());
    </script>
</asp:Content>
