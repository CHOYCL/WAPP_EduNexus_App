<%@ Page Title="My Discussions" Language="C#" MasterPageFile="~/MasterPages/HeaderTopBar_Footer.Master" AutoEventWireup="true" CodeBehind="MyDiscussions.aspx.cs" Inherits="WAPP_EduNexus_App.MyDiscussions" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="MyDiscussions.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="discussion-page" id="memberDiscussions">
        <section class="discussion-heading">
            <div>
                <p class="discussion-eyebrow">LEARN &bull; CONNECT &bull; GROW</p>
                <h1>Discussions</h1>
                <p class="discussion-intro">Ask a question about a course, share what you are learning, and help other learners.</p>
            </div>
            <button type="button" class="discussion-primary" id="newDiscussionButton">Ask a question</button>
        </section>

        <section class="discussion-stats" aria-label="Discussion summary">
            <article class="discussion-stat"><span>MY POSTS</span><strong id="memberPostCount">0</strong></article>
            <article class="discussion-stat"><span>REPLIES TO MY POSTS</span><strong id="memberReplyCount">0</strong></article>
            <article class="discussion-stat"><span>ACTIVE</span><strong id="memberActiveCount">0</strong></article>
        </section>

        <section class="discussion-list-section">
            <div class="discussion-list-heading">
                <div>
                    <h2>Course conversations</h2>
                    <p>Every post belongs to one course. Its author can mark it answered, and members or admins can keep replying afterward.</p>
                </div>
            </div>

            <div class="discussion-board-controls">
                <div class="discussion-tabs" role="tablist" aria-label="Discussion views">
                    <button type="button" class="discussion-tab active" role="tab" aria-selected="true" data-feed="all">All Discussions</button>
                    <button type="button" class="discussion-tab" role="tab" aria-selected="false" data-feed="mine">My Discussions</button>
                </div>
                <div class="discussion-filters">
                    <label class="discussion-search-wrap">
                        <span class="visually-hidden">Search discussions</span>
                        <input type="search" id="memberDiscussionSearch" placeholder="Search discussions..." />
                    </label>
                    <label class="visually-hidden" for="memberCourseFilter">Filter by course</label>
                    <asp:DropDownList ID="memberCourseFilter" runat="server" ClientIDMode="Static" CssClass="discussion-select" />
                    <label class="visually-hidden" for="memberStatusFilter">Filter by status</label>
                    <select id="memberStatusFilter">
                        <option value="all">All statuses</option>
                        <option value="open">Open</option>
                        <option value="answered">Answered</option>
                    </select>
                </div>
            </div>

            <div class="discussion-list" id="memberDiscussionList">
                <asp:Repeater ID="rptDiscussionList" runat="server" OnItemCommand="rptDiscussionList_ItemCommand" OnItemDataBound="rptDiscussionList_ItemDataBound">
                    <ItemTemplate>
                        <article class="discussion-card"
                            data-discussion-id='<%# Eval("DiscussionID") %>'
                            data-title='<%# EncodeAttribute(Eval("Title")) %>'
                            data-course-id='<%# Eval("CourseID") %>'
                            data-course='<%# EncodeAttribute(Eval("CourseTitle")) %>'
                            data-body='<%# EncodeAttribute(Eval("Details")) %>'
                            data-owner='<%# DiscussionOwner(Eval("UserID")) %>'
                            data-status='<%# Convert.ToString(Eval("Status")).ToLowerInvariant() %>'
                            data-replies='<%# Eval("ReplyCount") %>'>
                            <div class="discussion-card-top">
                                <span class="discussion-course"><%#: Eval("CourseTitle") %></span>
                                <span class='discussion-status status-<%# Convert.ToString(Eval("Status")).ToLowerInvariant() %>'><%#: Eval("Status") %></span>
                            </div>
                            <p class="discussion-author">Posted by <%#: Eval("Username") %></p>
                            <h3 class="discussion-title"><%#: Eval("Title") %></h3>
                            <p class="discussion-body"><%#: Eval("Details") %></p>
                            <div class="discussion-card-bottom">
                                <span class="discussion-reply-count"><%# Eval("ReplyCount") %> replies &bull; <%# FormatMalaysiaTime(Eval("CreatedAt"), "dd MMM yyyy") %></span>
                                <div class="discussion-actions">
                                    <button type="button" class="discussion-text-button" data-action="view">Open discussion</button>
                                    <asp:PlaceHolder ID="ownDiscussionActions" runat="server" Visible='<%# IsDiscussionOwner(Eval("UserID")) %>'>
                                        <asp:LinkButton ID="editMemberDiscussion" runat="server" CssClass="discussion-text-button" CommandName="Edit" CommandArgument='<%# Eval("DiscussionID") %>' CausesValidation="false">Edit</asp:LinkButton>
                                        <asp:LinkButton ID="deleteMemberDiscussion" runat="server" CssClass="discussion-text-button danger-text" CommandName="Delete" CommandArgument='<%# Eval("DiscussionID") %>' CausesValidation="false" OnClientClick="return confirm('Delete this discussion and its replies?');">Delete</asp:LinkButton>
                                    </asp:PlaceHolder>
                                    <asp:PlaceHolder ID="markAnsweredContainer" runat="server" Visible='<%# CanMarkAnswered(Eval("UserID"), Eval("Status")) %>'>
                                        <asp:LinkButton ID="markAnsweredButton" runat="server" CssClass="discussion-text-button" CommandName="MarkAnswered" CommandArgument='<%# Eval("DiscussionID") %>' CausesValidation="false">Mark answered</asp:LinkButton>
                                    </asp:PlaceHolder>
                                </div>
                            </div>
                            <div class="discussion-reply-source" hidden="hidden" aria-hidden="true">
                                <asp:Repeater ID="rptThreadReplies" runat="server">
                                    <ItemTemplate>
                                        <article class="thread-reply">
                                            <div class="thread-reply-author"><strong><%#: Eval("Username") %></strong><span><%#: ReplyRole(Eval("Role")) %></span></div>
                                            <p><%#: Eval("ReplyText") %></p>
                                            <time><%# FormatMalaysiaTime(Eval("CreatedAt"), "dd MMM yyyy, HH:mm") %></time>
                                        </article>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                        </article>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
            <div class="discussion-empty-state" id="memberDiscussionEmpty">
                <div class="discussion-empty-icon" aria-hidden="true">?</div>
                <h3>No discussions yet</h3>
                <p>Choose a course and start a conversation. Other learners can find your question and reply.</p>
                <button type="button" class="discussion-secondary" id="emptyStartDiscussion">Start the first discussion</button>
            </div>
        </section>
    </div>

    <div class="discussion-modal-backdrop" id="memberDiscussionModal" hidden>
        <section class="discussion-modal" role="dialog" aria-modal="true" aria-labelledby="memberModalTitle">
            <button type="button" class="discussion-modal-close" id="closeMemberModal" aria-label="Close">&times;</button>
            <p class="discussion-eyebrow">COURSE COMMUNITY</p>
            <h2 id="memberModalTitle">Start a discussion</h2>
            <p class="discussion-modal-intro">Choose one course. Your question will appear in that course&#39;s discussion feed.</p>
            <div id="memberDiscussionForm" role="form">
                <asp:HiddenField ID="memberDiscussionId" runat="server" ClientIDMode="Static" />
                <label for="memberDiscussionTitle">Title</label>
                <asp:TextBox ID="memberDiscussionTitle" runat="server" ClientIDMode="Static" MaxLength="120" required="required" placeholder="What would you like to discuss?" />
                <label for="memberDiscussionCourse">Course</label>
                <asp:DropDownList ID="memberDiscussionCourse" runat="server" ClientIDMode="Static" required="required" />
                <label for="memberDiscussionBody">Details</label>
                <asp:TextBox ID="memberDiscussionBody" runat="server" ClientIDMode="Static" TextMode="MultiLine" Rows="5" MaxLength="2000" required="required" placeholder="Add details to help others join the conversation..." />
                <div class="discussion-form-actions">
                    <button type="button" class="discussion-secondary" id="cancelMemberModal">Cancel</button>
                    <asp:Button ID="saveMemberDiscussion" runat="server" ClientIDMode="Static" CssClass="discussion-primary" Text="Post discussion" OnClick="saveMemberDiscussion_Click" />
                </div>
            </div>
        </section>
    </div>
    <asp:Literal ID="ltDiscussionNotice" runat="server" EnableViewState="false" />

    <div class="discussion-modal-backdrop" id="threadModal" hidden>
        <section class="discussion-modal thread-modal" role="dialog" aria-modal="true" aria-labelledby="threadTitle">
            <button type="button" class="discussion-modal-close" id="closeThreadModal" aria-label="Close">&times;</button>
            <p class="discussion-eyebrow" id="threadCourse"></p>
            <h2 id="threadTitle"></h2>
            <p class="thread-body" id="threadBody"></p>
            <section class="thread-replies">
                <h3>Replies</h3>
                <div id="threadReplyList"></div>
                <p id="threadEmptyReplies">No replies yet. Add the first reply.</p>
            </section>
            <label for="threadReplyInput">Your reply</label>
            <asp:HiddenField ID="threadDiscussionId" runat="server" ClientIDMode="Static" />
            <asp:TextBox ID="threadReplyInput" runat="server" ClientIDMode="Static" TextMode="MultiLine" Rows="3" MaxLength="2000" placeholder="Write a helpful reply..." />
            <div class="discussion-form-actions">
                <asp:Button ID="saveThreadReply" runat="server" CssClass="discussion-primary" Text="Post reply" OnClick="saveThreadReply_Click" CausesValidation="false" formnovalidate="formnovalidate" />
            </div>
        </section>
    </div>

    <script>
        (function () {
            var list = document.getElementById('memberDiscussionList');
            var modal = document.getElementById('memberDiscussionModal');
            var form = document.getElementById('memberDiscussionForm');
            var titleInput = document.getElementById('memberDiscussionTitle');
            var courseInput = document.getElementById('memberDiscussionCourse');
            var bodyInput = document.getElementById('memberDiscussionBody');
            var threadModal = document.getElementById('threadModal');
            var initialThreadId = <%= InitialThreadId %>;
            var initialEditId = <%= InitialEditId %>;

            function openComposer() {
                modal.hidden = false;
                titleInput.focus();
            }

            function closeModal() {
                modal.hidden = true;
                titleInput.value = '';
                courseInput.value = '';
                bodyInput.value = '';
                document.getElementById('memberDiscussionId').value = '';
                document.getElementById('memberModalTitle').textContent = 'Start a discussion';
                document.getElementById('saveMemberDiscussion').value = 'Post discussion';
            }

            function updateCount() {
                var cards = Array.prototype.slice.call(list.querySelectorAll('.discussion-card'));
                var myCards = cards.filter(function (card) { return card.dataset.owner === 'mine'; });
                document.getElementById('memberPostCount').textContent = myCards.length;
                document.getElementById('memberReplyCount').textContent = myCards.reduce(function (sum, card) {
                    return sum + parseInt(card.dataset.replies || '0', 10);
                }, 0);
                document.getElementById('memberActiveCount').textContent = myCards.filter(function (card) {
                    var status = card.querySelector('.discussion-status');
                    return status && status.classList.contains('status-open');
                }).length;
            }

            function applyFilters() {
                var query = document.getElementById('memberDiscussionSearch').value.trim().toLowerCase();
                var course = document.getElementById('memberCourseFilter').value;
                var status = document.getElementById('memberStatusFilter').value;
                var activeTab = document.querySelector('.discussion-tab.active').dataset.feed;
                var cards = Array.prototype.slice.call(list.querySelectorAll('.discussion-card'));
                var visibleCount = 0;

                cards.forEach(function (card) {
                    var text = (card.dataset.title + ' ' + card.dataset.course + ' ' + card.dataset.body).toLowerCase();
                    var matches = (!query || text.indexOf(query) !== -1) &&
                        (course === 'all' || card.dataset.courseId === course) &&
                        (status === 'all' || card.dataset.status === status) &&
                        (activeTab !== 'mine' || card.dataset.owner === 'mine');
                    card.hidden = !matches;
                    if (matches) visibleCount++;
                });

                var emptyState = document.getElementById('memberDiscussionEmpty');
                emptyState.hidden = visibleCount !== 0;
                if (cards.length > 0) {
                    emptyState.querySelector('h3').textContent = 'No discussions match these filters';
                    emptyState.querySelector('p').textContent = 'Try another course, status, or search phrase.';
                    document.getElementById('emptyStartDiscussion').hidden = true;
                } else {
                    emptyState.querySelector('h3').textContent = 'No discussions yet';
                    emptyState.querySelector('p').textContent = 'Choose a course and start a conversation. Other learners can find your question and reply.';
                    document.getElementById('emptyStartDiscussion').hidden = false;
                }
            }

            function openThread(card) {
                document.getElementById('threadCourse').textContent = card.dataset.course;
                document.getElementById('threadTitle').textContent = card.dataset.title;
                document.getElementById('threadBody').textContent = card.dataset.body;
                document.getElementById('threadReplyInput').value = '';
                document.getElementById('threadDiscussionId').value = card.dataset.discussionId;
                var replyList = document.getElementById('threadReplyList');
                replyList.textContent = '';
                var replyItems = card.querySelectorAll('.discussion-reply-source .thread-reply');
                Array.prototype.forEach.call(replyItems, function (reply) {
                    replyList.appendChild(reply.cloneNode(true));
                });
                document.getElementById('threadEmptyReplies').hidden = replyItems.length > 0;
                threadModal.hidden = false;
                document.getElementById('threadReplyInput').focus();
            }

            document.getElementById('newDiscussionButton').addEventListener('click', openComposer);
            document.getElementById('emptyStartDiscussion').addEventListener('click', openComposer);
            document.getElementById('closeMemberModal').addEventListener('click', closeModal);
            document.getElementById('cancelMemberModal').addEventListener('click', closeModal);
            modal.addEventListener('click', function (event) {
                if (event.target === modal) closeModal();
            });
            document.getElementById('closeThreadModal').addEventListener('click', function () { threadModal.hidden = true; });
            threadModal.addEventListener('click', function (event) {
                if (event.target === threadModal) threadModal.hidden = true;
            });
            document.querySelectorAll('.discussion-tab').forEach(function (tab) {
                tab.addEventListener('click', function () {
                    document.querySelectorAll('.discussion-tab').forEach(function (item) {
                        item.classList.remove('active');
                        item.setAttribute('aria-selected', 'false');
                    });
                    tab.classList.add('active');
                    tab.setAttribute('aria-selected', 'true');
                    applyFilters();
                });
            });

            list.addEventListener('click', function (event) {
                var button = event.target.closest('button[data-action]');
                if (!button) return;
                var card = button.closest('.discussion-card');
                if (button.dataset.action === 'view') {
                    openThread(card);
                }
            });

            form.addEventListener('keydown', function (event) {
                if (event.key === 'Enter' && event.target.tagName === 'INPUT') event.preventDefault();
            });

            document.getElementById('saveMemberDiscussion').addEventListener('click', function (event) {
                var requiredFields = [titleInput, courseInput, bodyInput];
                for (var fieldIndex = 0; fieldIndex < requiredFields.length; fieldIndex++) {
                    if (!requiredFields[fieldIndex].checkValidity()) {
                        event.preventDefault();
                        requiredFields[fieldIndex].reportValidity();
                        return;
                    }
                }
            });

            document.getElementById('memberDiscussionSearch').addEventListener('input', applyFilters);
            document.getElementById('memberCourseFilter').addEventListener('change', applyFilters);
            document.getElementById('memberStatusFilter').addEventListener('change', applyFilters);
            updateCount();
            applyFilters();
            if (initialEditId > 0) {
                document.getElementById('memberModalTitle').textContent = 'Edit discussion';
                document.getElementById('saveMemberDiscussion').value = 'Save changes';
                openComposer();
            }
            if (initialThreadId > 0) {
                var initialThread = list.querySelector('[data-discussion-id="' + initialThreadId + '"]');
                if (initialThread) openThread(initialThread);
            }
        }());
    </script>
</asp:Content>
