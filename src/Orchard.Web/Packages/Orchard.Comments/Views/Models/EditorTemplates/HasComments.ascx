<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<HasComments>" %>
<%@ Import Namespace="Orchard.Comments.Models"%>
<fieldset>
    <legend>Comments<% if (Model.ContentItem.Id != 0) { %>: <% var commentCount = Model.Comments.Count(); %>
    <%=Html.ActionLink(
            string.Format("{0} comment{1}", commentCount, commentCount == 1 ? "" : "s"),
            "Details",
            new { Area = "Orchard.Comments", Controller = "Admin", id = Model.ContentItem.Id, returnUrl = Context.Request.Url }
            ) %>
    - <a href="#">0 pending</a><% } %></legend>
    <label for="Closed"><%=Html.EditorFor(hc => hc.Closed) %> Close comments</label>
</fieldset>
<%--
todo: (heskew) would be better to have a ↑ checkbox ↑ instead of a ↓ button ↓
<div class="actions">
    <% if (Model.Closed) {
        %><%=Html.ActionLink("Enable Comments", "Enable", new { commentedItemId = Model.Closed }, new { @class = "button" })%><%
       } else {
        %><%=Html.ActionLink("Close Comments", "Close", new { commentedItemId = Model.Closed }, new { @class = "button remove" })%><%
       } %>
</div>
--%><%--
todo: (heskew) shouldn't have comments when editing a content item. besides being noisy/distracting it throw other issues like paging into the mix
<ol class="contentItems">
    <% foreach (var comment in Model.Comments) {%>
    <li>
        <p><%= comment.CommentText %><br />
            Posted by <%= comment.UserName %> on <%= comment.CommentDate.ToLocalTime() %><br />
	        <%=Html.ActionLink("Delete", "Delete", new { Area = "Orchard.Comments", Controller = "Admin", id = comment.Id, returnUrl = Context.Request.Url }, new { @class = "ibutton remove" })%></p>
    </li>
    <% } %>
</ol>--%>
