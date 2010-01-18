<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<CommentsEditViewModel>" %>
<%@ Import Namespace="Orchard.Comments.Models"%>
<%@ Import Namespace="Orchard.Comments.ViewModels"%>
<h1><%=Html.TitleForPage(T("Edit Comment").ToString())%></h1>
<% using(Html.BeginFormAntiForgeryPost()) { %>
    <%= Html.ValidationSummary() %>
    <fieldset class="who">
        <div>
            <label for="Name"><%=_Encoded("Name") %></label>
            <input id="Name" class="text" name="Name" type="text" value="<%=Html.Encode(Model.Name) %>" />
        </div>
        <div>
            <label for="Email"><%=_Encoded("Email") %></label>
            <input id="Email" class="text" name="Email" type="text" value="<%=Html.Encode(Model.Email)%>" />				
        </div>
        <div>
            <label for="SiteName"><%=_Encoded("Url") %></label>
            <input id="SiteName" class="text" name="SiteName" type="text" value="<%=Html.Encode(Model.SiteName) %>" />
        </div>
    </fieldset>
    <fieldset class="what">
        <div>
            <label for="CommentText"><%=_Encoded("Body") %></label>
            <textarea id="CommentText" rows="10" cols="30" name="CommentText"><%=Html.Encode(Model.CommentText) %></textarea>
	        <input id="CommentId" name="Id" type="hidden" value="<%=Model.Id %>" />
        </div>
    </fieldset>
    <fieldset>
        <div>
            <%=Html.RadioButton("Status", "Pending", (Model.Status == CommentStatus.Pending), new { id = "Status_Pending" }) %> 
            <label class="forcheckbox" for="Status_Pending"><%=_Encoded("Pending") %></label>
        </div>
        <div>
            <%=Html.RadioButton("Status", "Approved", (Model.Status == CommentStatus.Approved), new { id = "Status_Approved" }) %>
            <label class="forcheckbox" for="Status_Approved"><%=_Encoded("Approved") %></label>
        </div>
        <div>
            <%=Html.RadioButton("Status", "Spam", (Model.Status == CommentStatus.Spam), new { id = "Status_Spam" }) %> 
            <label class="forcheckbox" for="Status_Spam"><%=_Encoded("Mark as spam") %></label>
        </div>
    </fieldset>
    <fieldset>
	    <input type="submit" class="button" value="<%=_Encoded("Save") %>" />
    </fieldset>
<% } %>