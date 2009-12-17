<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<CommentsEditViewModel>" %>
<%@ Import Namespace="Orchard.Comments.Models"%>
<%@ Import Namespace="Orchard.Comments.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<h2>Edit Comment</h2>
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
	    <textarea id="Textarea1" rows="10" cols="30" name="CommentText"><%=Model.CommentText %></textarea>
	    <input id="CommentId" name="Id" type="hidden" value="<%=Model.Id %>" />
    </fieldset>
    <fieldset>
        <label for="Status_Approved">
            <%=Html.RadioButton("Status", "Approved", (Model.Status == CommentStatus.Approved), new { id = "Status_Approved" }) %> Approved
        </label>
        <label for="Status_Spam">
            <%=Html.RadioButton("Status", "Spam", (Model.Status == CommentStatus.Spam), new { id = "Status_Spam" }) %> Mark As Spam
        </label>
    </fieldset>
    <fieldset>
	    <input type="submit" class="button" value="Save" />
    </fieldset>
<% } %>