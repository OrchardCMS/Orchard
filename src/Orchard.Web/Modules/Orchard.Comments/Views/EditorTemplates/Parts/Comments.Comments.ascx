<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<CommentsPart>" %>
<%@ Import Namespace="Orchard.Comments.Extensions"%>
<%@ Import Namespace="Orchard.Localization" %>
<%@ Import Namespace="Orchard.Comments.Models" %>
<fieldset>
    <%-- todo: (heskew) pull the legend and put the link to the comments elsewhere? --%>
    <legend><%: T("Comments")%><% if (Model.Comments.Count > 0) { %> <span>&#150; <%: Html.CommentSummaryLinks(T, Model.ContentItem, Model.Comments.Count, Model.PendingComments.Count)%></span><% } %></legend>
    <%--
    todo: (heskew) can get into a weird state if this is disabled but comments are active so, yeah, comment settings on a content item need to be hashed out
    <%: Html.EditorFor(m => m.CommentsShown) %>
    <label class="forcheckbox" for="CommentsShown"><%: T("Comments are shown. Existing comments are displayed.") %></label>
--%>
    <%: Html.EditorFor(m => m.CommentsActive) %>
    <label class="forcheckbox" for="CommentsActive">
        <%: T("Allow new comments") %></label> <span class="hint forcheckbox">
            <%: T("Enable to show the comment form. Disabling still allows the existing comments to be shown but does not allow the conversation to continue.")%></span>
</fieldset>