<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/AdminHeader_Footer.Master" AutoEventWireup="true" CodeBehind="AdminManageQuiz.aspx.cs" Inherits="WAPP_EduNexus_App.AdminManageQuiz" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder2" runat="server">
    <link href="<%= ResolveUrl("~/Content/QuizModule.css") %>" rel="stylesheet" />

    <div class="page-container">
        <div class="qz-page">
            <asp:HyperLink ID="lnkBack" runat="server" CssClass="qz-back" Text="&larr; Back to modules" />
            <h1 class="qz-title">Quiz questions</h1>
            <p class="qz-sub">Module: <strong><asp:Literal ID="ltModuleTitle" runat="server" /></strong> &ndash; <asp:Literal ID="ltSummary" runat="server" /></p>

            <asp:Label ID="lblMessage" runat="server" Visible="false" EnableViewState="false" />

            <!-- Add / edit form -->
            <div class="qz-card">
                <h2 class="qz-card-title"><asp:Literal ID="ltFormTitle" runat="server" /></h2>
                <asp:HiddenField ID="hfQuestionId" runat="server" />

                <div class="qz-row">
                    <div class="qz-field">
                        <label for="<%= ddlType.ClientID %>">Question type</label>
                        <asp:DropDownList ID="ddlType" runat="server" CssClass="qz-input" onchange="toggleQuestionType()">
                            <asp:ListItem Value="MCQ">Multiple choice</asp:ListItem>
                            <asp:ListItem Value="Short">Short answer</asp:ListItem>
                            <asp:ListItem Value="Long">Long answer</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="qz-field">
                        <label for="<%= txtMarks.ClientID %>">Marks</label>
                        <asp:TextBox ID="txtMarks" runat="server" CssClass="qz-input" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtMarks" ValidationGroup="vgQuestion"
                            Display="Dynamic" CssClass="qz-error" ErrorMessage="Marks are required." />
                        <asp:RangeValidator runat="server" ControlToValidate="txtMarks" ValidationGroup="vgQuestion"
                            Type="Integer" MinimumValue="1" MaximumValue="100" Display="Dynamic" CssClass="qz-error"
                            ErrorMessage="Enter a whole number from 1 to 100." />
                    </div>
                </div>

                <div class="qz-field">
                    <label for="<%= txtQuestion.ClientID %>">Question</label>
                    <asp:TextBox ID="txtQuestion" runat="server" CssClass="qz-input" TextMode="MultiLine" Rows="3" MaxLength="500" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtQuestion" ValidationGroup="vgQuestion"
                        Display="Dynamic" CssClass="qz-error" ErrorMessage="The question text is required." />
                </div>

                <!-- Multiple choice -->
                <div id="pnlMcq">
                    <div class="qz-row">
                        <div class="qz-field">
                            <label for="<%= txtOptA.ClientID %>">Option A</label>
                            <asp:TextBox ID="txtOptA" runat="server" CssClass="qz-input" MaxLength="250" />
                        </div>
                        <div class="qz-field">
                            <label for="<%= txtOptB.ClientID %>">Option B</label>
                            <asp:TextBox ID="txtOptB" runat="server" CssClass="qz-input" MaxLength="250" />
                        </div>
                    </div>
                    <div class="qz-row">
                        <div class="qz-field">
                            <label for="<%= txtOptC.ClientID %>">Option C (optional)</label>
                            <asp:TextBox ID="txtOptC" runat="server" CssClass="qz-input" MaxLength="250" />
                        </div>
                        <div class="qz-field">
                            <label for="<%= txtOptD.ClientID %>">Option D (optional)</label>
                            <asp:TextBox ID="txtOptD" runat="server" CssClass="qz-input" MaxLength="250" />
                        </div>
                    </div>
                    <div class="qz-field">
                        <label for="<%= ddlCorrect.ClientID %>">Correct option</label>
                        <asp:DropDownList ID="ddlCorrect" runat="server" CssClass="qz-input qz-input-sm">
                            <asp:ListItem>A</asp:ListItem>
                            <asp:ListItem>B</asp:ListItem>
                            <asp:ListItem>C</asp:ListItem>
                            <asp:ListItem>D</asp:ListItem>
                        </asp:DropDownList>
                        <span class="qz-help">Use two options for a true/false question.</span>
                    </div>
                </div>

                <!-- Short answer -->
                <div id="pnlShort" style="display:none;">
                    <div class="qz-field">
                        <label for="<%= txtShort.ClientID %>">Accepted answers</label>
                        <asp:TextBox ID="txtShort" runat="server" CssClass="qz-input" TextMode="MultiLine" Rows="3" />
                        <span class="qz-help">One per line, or separated by |. Matching ignores capital letters and extra spaces. Marked automatically.</span>
                    </div>
                </div>

                <!-- Long answer -->
                <div id="pnlLong" style="display:none;">
                    <div class="qz-field">
                        <label for="<%= txtGuide.ClientID %>">Marking guide (optional)</label>
                        <asp:TextBox ID="txtGuide" runat="server" CssClass="qz-input" TextMode="MultiLine" Rows="4" />
                        <span class="qz-help">Only you see this. Long answers are marked by hand on the Attempts page.</span>
                    </div>
                </div>

                <asp:CustomValidator ID="cvQuestion" runat="server" ValidationGroup="vgQuestion" Display="Dynamic"
                    CssClass="qz-error" ClientValidationFunction="validateQuestionFields"
                    OnServerValidate="cvQuestion_ServerValidate" ErrorMessage="Complete the fields for this question type." />

                <span class="qz-help">Changing a question that students have already answered does not re-mark their old attempts.</span>
                <br />
                <asp:Button ID="btnSave" runat="server" Text="Add question" CssClass="qz-btn qz-btn-primary"
                    ValidationGroup="vgQuestion" OnClick="btnSave_Click" />
                <asp:Button ID="btnCancel" runat="server" Text="Cancel edit" CssClass="qz-btn qz-btn-light"
                    CausesValidation="false" Visible="false" OnClick="btnCancel_Click" />
            </div>

            <!-- Question list -->
            <div class="qz-card">
                <h2 class="qz-card-title">Questions in this module</h2>
                <asp:Literal ID="ltEmpty" runat="server" />

                <asp:Repeater ID="rptQuestions" runat="server" OnItemCommand="rptQuestions_ItemCommand">
                    <HeaderTemplate>
                        <table class="qz-table">
                            <thead>
                                <tr>
                                    <th>#</th>
                                    <th>Type</th>
                                    <th>Question</th>
                                    <th>Answer key</th>
                                    <th>Marks</th>
                                    <th></th>
                                </tr>
                            </thead>
                            <tbody>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td><%# Container.ItemIndex + 1 %></td>
                            <td><span class='qz-tag qz-tag-<%# Eval("questionType") %>'><%# TypeLabel(Eval("questionType")) %></span></td>
                            <td><%#: Eval("questionText") %></td>
                            <td><%#: AnswerKey(Container.DataItem) %></td>
                            <td><%# Eval("marks") %></td>
                            <td class="qz-actions">
                                <asp:Button runat="server" Text="Edit" CssClass="qz-btn qz-btn-light"
                                    CommandName="EditQuestion" CommandArgument='<%# Eval("questionID") %>' />
                                <asp:Button runat="server" Text="Delete" CssClass="qz-btn qz-btn-danger"
                                    CommandName="DeleteQuestion" CommandArgument='<%# Eval("questionID") %>'
                                    OnClientClick="return confirm('Delete this question? The answers students gave to it will be removed too.');" />
                            </td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                            </tbody>
                        </table>
                    </FooterTemplate>
                </asp:Repeater>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        // @ts-nocheck
        // Show only the fields that belong to the chosen question type.
        function toggleQuestionType() {
            var type = document.getElementById('<%= ddlType.ClientID %>').value;
            document.getElementById('pnlMcq').style.display = (type === 'MCQ') ? 'block' : 'none';
            document.getElementById('pnlShort').style.display = (type === 'Short') ? 'block' : 'none';
            document.getElementById('pnlLong').style.display = (type === 'Long') ? 'block' : 'none';
        }

        // Client-side check; the same rules run again on the server in cvQuestion_ServerValidate.
        function validateQuestionFields(source, args) {
            var type = document.getElementById('<%= ddlType.ClientID %>').value;
            var optionIds = {
                A: '<%= txtOptA.ClientID %>',
                B: '<%= txtOptB.ClientID %>',
                C: '<%= txtOptC.ClientID %>',
                D: '<%= txtOptD.ClientID %>'
            };

            function valueOf(id) {
                return document.getElementById(id).value.replace(/^\s+|\s+$/g, '');
            }

            var message = '';
            if (type === 'MCQ') {
                var correct = document.getElementById('<%= ddlCorrect.ClientID %>').value;
                if (!valueOf(optionIds.A) || !valueOf(optionIds.B)) {
                    message = 'Multiple choice needs at least options A and B.';
                } else if (!valueOf(optionIds[correct])) {
                    message = 'The correct option (' + correct + ') must have text.';
                }
            } else if (type === 'Short') {
        if (!valueOf('<%= txtShort.ClientID %>')) {
            message = 'Enter at least one accepted answer.';
        }
    }

    args.IsValid = (message === '');
    if (message) {
        source.errormessage = message;
        source.innerHTML = message;
    }
}

toggleQuestionType();
</script>
</asp:Content>
