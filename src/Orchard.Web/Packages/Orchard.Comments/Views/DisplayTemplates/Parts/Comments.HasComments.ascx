<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<HasComments>" %>
<%@ Import Namespace="Orchard.Comments.Models"%>
<h3 id="comments"><a name="comments"><%=Model.Comments.Count() %> Comment<%=Model.Comments.Count() == 1 ? "" : "s" %></a></h3><%
    foreach (var comment in Model.Comments) { %>
<div>
    <div class="comment">
        <%--TODO: (erikpo) Need to clean the name and url so nothing dangerous goes out--%>
        <span class="who"><%=Html.LinkOrDefault(comment.UserName, comment.SiteName, new { rel = "nofollow" })%></span>
        <span>said <%=Html.Link(Html.DateTimeRelative(comment.CommentDate), "#")%></span>
    </div>
    <div class="text">
        <p><%=Html.Encode(comment.CommentText) %></p>
    </div>
</div><%
    }
if (Model.Closed) { %>
    <p>Comments have been disabled for this content.</p><%
} else { %>
    <% using(Html.BeginForm("Create", "Admin", new { area = "Orchard.Comments" }, FormMethod.Post, new { @class = "comments" })) { %>
        <%=Html.ValidationSummary() %>
        <fieldset class="who">
            <%= Html.Hidden("CommentedOn", Model.ContentItem.Id) %>
            <%= Html.Hidden("ReturnUrl", Context.Request.Url) %>
            <label for="Name">Name</label>
            <input id="Name" class="text" name="Name" type="text" /><br />
            <label for="Email">Email</label>
            <input id="Email" class="text" name="Email" type="text" /><br />				
            <label for="SiteName">Url</label>
            <input id="SiteName" class="text" name="SiteName" type="text" /><br />
        </fieldset>
        <fieldset class="what">
            <label for="CommentText">Leave a comment</label>
            <textarea id="CommentText" rows="10" cols="30" name="CommentText"></textarea><br />
            <input type="submit" class="button" value="Submit Comment" />
		    <%=Html.AntiForgeryTokenOrchard() %>
        </fieldset><%
   }
} %>