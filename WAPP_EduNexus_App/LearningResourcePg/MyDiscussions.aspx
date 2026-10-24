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
                    <p>Every post belongs to one course. Select a discussion to join the conversation.</p>
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
                    <select id="memberCourseFilter">
                        <option value="all">All courses</option>
                        <option value="Night Photography">Night Photography</option>
                        <option value="Digital Design">Digital Design</option>
                        <option value="JavaScript Basics">JavaScript Basics</option>
                    </select>
                    <label class="visually-hidden" for="memberStatusFilter">Filter by status</label>
                    <select id="memberStatusFilter">
                        <option value="all">All statuses</option>
                        <option value="open">Open</option>
                        <option value="answered">Answered</option>
                        <option value="closed">Closed</option>
                    </select>
                </div>
            </div>

            <div class="discussion-list" id="memberDiscussionList">
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
            <p class="discussion-modal-intro">Choose one course. Your question will appear in that course’s discussion feed.</p>
            <div id="memberDiscussionForm" role="form">
                <label for="memberDiscussionTitle">Title</label>
                <input id="memberDiscussionTitle" name="title" maxlength="120" required placeholder="What would you like to discuss?" />
                <label for="memberDiscussionCourse">Course</label>
                <select id="memberDiscussionCourse" name="course" required>
                    <option value="">Choose a course</option>
                    <option value="Night Photography">Night Photography</option>
                    <option value="Digital Design">Digital Design</option>
                    <option value="JavaScript Basics">JavaScript Basics</option>
                </select>
                <label for="memberDiscussionBody">Details</label>
                <textarea id="memberDiscussionBody" name="body" rows="5" maxlength="2000" required placeholder="Add details to help others join the conversation..."></textarea>
                <div class="discussion-form-actions">
                    <button type="button" class="discussion-secondary" id="cancelMemberModal">Cancel</button>
                    <button type="button" class="discussion-primary" id="saveMemberDiscussion">Post discussion</button>
                </div>
            </div>
        </section>
    </div>

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
            <textarea id="threadReplyInput" rows="3" maxlength="1000" placeholder="Write a helpful reply..."></textarea>
            <div class="discussion-form-actions">
                <button type="button" class="discussion-primary" id="postThreadReply">Post reply</button>
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
            var currentThreadCard = null;
            var editingCard = null;

            function openComposer() {
                modal.hidden = false;
                titleInput.focus();
            }

            function closeModal() {
                modal.hidden = true;
                titleInput.value = '';
                courseInput.value = '';
                bodyInput.value = '';
                editingCard = null;
                document.getElementById('memberModalTitle').textContent = 'Start a discussion';
                document.getElementById('saveMemberDiscussion').textContent = 'Post discussion';
            }

            function makeAction(label, action, extraClass) {
                var button = document.createElement('button');
                button.type = 'button';
                button.className = 'discussion-text-button' + (extraClass ? ' ' + extraClass : '');
                button.dataset.action = action;
                button.textContent = label;
                return button;
            }

            function makeCard(title, course, body) {
                var card = document.createElement('article');
                card.className = 'discussion-card';
                card.dataset.title = title;
                card.dataset.course = course;
                card.dataset.body = body;
                card.dataset.owner = 'mine';
                card.dataset.status = 'open';
                card.dataset.replies = '0';
                card._replies = [];

                var top = document.createElement('div');
                top.className = 'discussion-card-top';
                var courseTag = document.createElement('span');
                courseTag.className = 'discussion-course';
                courseTag.textContent = course;
                var status = document.createElement('span');
                status.className = 'discussion-status status-open';
                status.textContent = 'Open';
                top.appendChild(courseTag);
                top.appendChild(status);

                var author = document.createElement('p');
                author.className = 'discussion-author';
                author.textContent = 'Posted by you';

                var heading = document.createElement('h3');
                heading.className = 'discussion-title';
                heading.textContent = title;
                var description = document.createElement('p');
                description.className = 'discussion-body';
                description.textContent = body;
                var bottom = document.createElement('div');
                bottom.className = 'discussion-card-bottom';
                var meta = document.createElement('span');
                meta.className = 'discussion-reply-count';
                meta.textContent = '0 replies • Just now';
                var actions = document.createElement('div');
                actions.className = 'discussion-actions';
                actions.appendChild(makeAction('Open discussion', 'view'));
                actions.appendChild(makeAction('Edit', 'edit'));
                actions.appendChild(makeAction('Delete', 'delete', 'danger-text'));
                bottom.appendChild(meta);
                bottom.appendChild(actions);

                card.appendChild(top);
                card.appendChild(author);
                card.appendChild(heading);
                card.appendChild(description);
                card.appendChild(bottom);
                return card;
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
                        (course === 'all' || card.dataset.course === course) &&
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
                currentThreadCard = card;
                document.getElementById('threadCourse').textContent = card.dataset.course;
                document.getElementById('threadTitle').textContent = card.dataset.title;
                document.getElementById('threadBody').textContent = card.dataset.body;
                document.getElementById('threadReplyInput').value = '';
                var replyList = document.getElementById('threadReplyList');
                replyList.textContent = '';
                (card._replies || []).forEach(function (reply) {
                    var replyItem = document.createElement('article');
                    replyItem.className = 'thread-reply';
                    var author = document.createElement('strong');
                    author.textContent = 'You';
                    var text = document.createElement('p');
                    text.textContent = reply;
                    replyItem.appendChild(author);
                    replyItem.appendChild(text);
                    replyList.appendChild(replyItem);
                });
                document.getElementById('threadEmptyReplies').hidden = (card._replies || []).length > 0;
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
            document.getElementById('postThreadReply').addEventListener('click', function () {
                var replyInput = document.getElementById('threadReplyInput');
                var reply = replyInput.value.trim();
                if (!reply || !currentThreadCard) {
                    replyInput.focus();
                    return;
                }
                currentThreadCard._replies = currentThreadCard._replies || [];
                currentThreadCard._replies.push(reply);
                currentThreadCard.dataset.replies = currentThreadCard._replies.length;
                currentThreadCard.querySelector('.discussion-reply-count').textContent = currentThreadCard.dataset.replies + ' replies • Just now';
                updateCount();
                openThread(currentThreadCard);
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
                    return;
                }
                if (button.dataset.action === 'delete') {
                    if (window.confirm('Delete this discussion?')) {
                        card.remove();
                        updateCount();
                        applyFilters();
                    }
                    return;
                }
                editingCard = card;
                titleInput.value = card.dataset.title;
                courseInput.value = card.dataset.course;
                bodyInput.value = card.dataset.body;
                document.getElementById('memberModalTitle').textContent = 'Edit discussion';
                document.getElementById('saveMemberDiscussion').textContent = 'Save changes';
                modal.hidden = false;
                titleInput.focus();
            });

            form.addEventListener('keydown', function (event) {
                if (event.key === 'Enter' && event.target.tagName === 'INPUT') event.preventDefault();
            });

            document.getElementById('saveMemberDiscussion').addEventListener('click', function () {
                var requiredFields = [titleInput, courseInput, bodyInput];
                for (var fieldIndex = 0; fieldIndex < requiredFields.length; fieldIndex++) {
                    if (!requiredFields[fieldIndex].checkValidity()) {
                        requiredFields[fieldIndex].reportValidity();
                        return;
                    }
                }
                var title = titleInput.value.trim();
                var course = courseInput.value;
                var body = bodyInput.value.trim();
                if (!title || !course || !body) return;

                if (editingCard) {
                    editingCard.dataset.title = title;
                    editingCard.dataset.course = course;
                    editingCard.dataset.body = body;
                    editingCard.querySelector('.discussion-course').textContent = course;
                    editingCard.querySelector('.discussion-title').textContent = title;
                    editingCard.querySelector('.discussion-body').textContent = body;
                } else {
                    list.insertBefore(makeCard(title, course, body), list.firstChild);
                    updateCount();
                }
                closeModal();
                applyFilters();
            });

            document.getElementById('memberDiscussionSearch').addEventListener('input', applyFilters);
            document.getElementById('memberCourseFilter').addEventListener('change', applyFilters);
            document.getElementById('memberStatusFilter').addEventListener('change', applyFilters);
            applyFilters();
        }());
    </script>
</asp:Content>
