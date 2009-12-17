<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<CommentsCreateViewModel>" %>
<%@ Import Namespace="Orchard.Comments.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<h2>Add Comment</h2>
<% using(Html.BeginForm()) { %>
    <%= Html.ValidationSummary() %>
    <fieldset class="who">
        <label for="CommentName">Name</label>
	    <input id="CommentName" class="text" name="Name" type="text" value="<%=Model.Name %>" /><br />
        <label for="CommentEmail">Email</label>
        <input id="CommentEmail" class="text" name="Email" type="text" value="<%=Model.Email%>" />	<br />				
        <label for="CommentSiteName">SiteName</label>
        <input id="CommentSiteName" class="text" name="SiteName" type="text" value="<%=Model.SiteName %>" />
    </fieldset>
    <fieldset class="what">
        <label for="CommentText">Leave a comment</label>
	    <textarea id="CommentText" rows="10" cols="30" name="CommentText"><%=Model.CommentText %></textarea>
	    <input type="submit" class="button" value="Save" />
    </fieldset>
<% } %>