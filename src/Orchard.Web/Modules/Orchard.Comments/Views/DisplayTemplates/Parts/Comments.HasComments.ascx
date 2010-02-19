<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<HasComments>" %>
<%@ Import Namespace="Orchard.Security" %>
<%@ Import Namespace="Orchard.Comments.Models" %>
<h2 id="comments">
    <%=_Encoded("{0} Comment{1}", Model.Comments.Count, Model.Comments.Count == 1 ? "" : "s")%></h2>
<%
    if (Model.Comments.Count > 0) { Html.RenderPartial("ListOfComments", Model.Comments); }
    if (Model.CommentsActive == false) { %>
<p>
    <%=_Encoded("Comments have been disabled for this content.") %></p>
<%
    }
    else { %>
<% using (Html.BeginForm("Create", "Comment", new { area = "Orchard.Comments" }, FormMethod.Post, new { @class = "comment" })) { %>
<%=Html.ValidationSummary() %>
<% 
    if (!Request.IsAuthenticated) { %>
<fieldset class="who">
    <div>
        <label for="Name">
            <%=_Encoded("Name") %></label>
        <input id="Name" class="text" name="Name" type="text" />
    </div>
    <div>
        <label for="Email">
            <%=_Encoded("Email") %></label>
        <input id="Email" class="text" name="Email" type="text" />
    </div>
    <div>
        <label for="SiteName">
            <%=_Encoded("Url") %></label>
        <input id="SiteName" class="text" name="SiteName" type="text" />
    </div>
</fieldset>
<%    
    }
    else {
        var currentUser = Html.Resolve<IAuthenticationService>().GetAuthenticatedUser();
%>
        <%=Html.Hidden("Name", currentUser.UserName ?? "")%>
        <%=Html.Hidden("Email", currentUser.Email ?? "")%>
<%
    }%>
<fieldset class="what">
    <div>
        <label for="CommentText">
            <% if (Request.IsAuthenticated) { %><%=T("Hi, {0}!", Html.Encode(Page.User.Identity.Name)) %><br />
            <% } %><%=_Encoded("Leave a comment") %></label>
        <textarea id="CommentText" rows="10" cols="30" name="CommentText"></textarea>
    </div>
    <div>
        <input type="submit" class="button" value="<%=_Encoded("Submit Comment") %>" />
        <%=Html.Hidden("CommentedOn", Model.ContentItem.Id) %>
        <%=Html.Hidden("ReturnUrl", Context.Request.Url) %>
        <%=Html.AntiForgeryTokenOrchard() %>
    </div>
</fieldset>
<%
    }
    } %>
