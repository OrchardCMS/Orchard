<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<HasComments>" %>
<%@ Import Namespace="Orchard.Comments.Models"%>
<h3><%= Model.Comments.Count() %> Comments</h3>
<ol>
    <% foreach (var comment in Model.Comments) {%>
    <li>
        <%= comment.CommentText %>
    </li>
    <li>
        Posted by <%= comment.UserName %> on <%= comment.CommentDate.ToLocalTime() %>
    </li>
    <li>
	    <%=Html.ActionLink("Delete", "Delete", new {Area="Orchard.Comments", Controller="Admin", id = comment.Id, returnUrl = Context.Request.Url}) %>
    </li>
    <hr />
    <% } %>
</ol>
<% if (Model.Closed) { %>
<p>Comments have been disabled for this content.</p>
<%= Html.ActionLink("Enable Comments for this content", "Enable", new { Area="Orchard.Comments", Controller="Admin", returnUrl = Context.Request.Url, commentedItemId = Model.ContentItem.Id })%>
<% } else { %>
<%= Html.ActionLink("Close Comments for this content", "Close", new { Area="Orchard.Comments", Controller="Admin", returnUrl = Context.Request.Url, commentedItemId = Model.ContentItem.Id })%>
<% } %> | <%=Html.ActionLink("Go to comments management for this post", "Details", new {Area="Orchard.Comments", Controller="Admin", id = Model.ContentItem.Id, returnUrl = Context.Request.Url}) %>
