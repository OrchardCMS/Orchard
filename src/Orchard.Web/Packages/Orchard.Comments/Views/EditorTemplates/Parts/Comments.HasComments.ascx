<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<HasComments>" %>
<%@ Import Namespace="Orchard.Comments.Models" %>
<fieldset>
    <legend>Comments<% if (Model.ContentItem.Id != 0) { %>:
        <%
            var commentCount = Model.CommentCount; %>
        <%=Html.ActionLink(
            string.Format("{0} comment{1}", commentCount, commentCount == 1 ? "" : "s"),
            "Details",
            new { Area = "Orchard.Comments", Controller = "Admin", id = Model.ContentItem.Id, returnUrl = Context.Request.Url }
            ) %>
        - <a href="#">0 pending</a><% } %></legend>
    <label for="CommentsShown">
        <%=Html.EditorFor(x=>x.CommentsShown) %>
        <%= T("Comments are shown. Existing comments are displayed.")%>
    </label>
    <label for="CommentsActive">
        <%=Html.EditorFor(x=>x.CommentsActive) %>
        <%= T("Comments active. Users may add comments.")%>
    </label>
</fieldset>
