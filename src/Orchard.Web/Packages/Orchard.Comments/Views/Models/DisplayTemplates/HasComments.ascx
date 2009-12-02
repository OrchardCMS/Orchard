<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<HasComments>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Comments.Models"%>
<h3><%=Model.Comments.Count() %> Comment<%=Model.Comments.Count() == 1 ? "" : "s" %></h3><%
    foreach (var comment in Model.Comments) { %>
<div class="name">
    <div><!-- GRAVATAR --></div>
    <p class="comment">
        <%--TODO: (erikpo) Need to clean the name and url so nothing dangerous goes out--%>
        <strong><%=Html.LinkOrDefault(comment.UserName, comment.SiteName, new { rel = "nofollow" })%></strong>
        <span>said<br /><%=Html.Link(Html.DateTime(comment.CommentDate), "#")%></span>
    </p>
    <div class="text">
        <p><%=comment.CommentText %></p>
    </div>
</div><%
    }
    if (Model.Closed) { %>
<p>Comments have been disabled for this content.</p><%
    }
    else { %>
<% Html.BeginForm("Create", "Admin", new { area = "Orchard.Comments" }); %>
<%=Html.ValidationSummary() %>
<div class="yui-g">
    <h2 class="separator">Add a Comment</h2>
    <ol>
        <li>
            <%= Html.Hidden("CommentedOn", Model.ContentItem.Id) %>
            <%= Html.Hidden("ReturnUrl", Context.Request.Url) %>
            <label for="Name">Name:</label>
            <input id="Text1" class="inputText inputTextLarge" name="Name" type="text" />
        </li>
        <li>
            <label for="Email">Email:</label>
            <input id="Email" class="inputText inputTextLarge" name="Email" type="text" />					
        </li>
        <li>
            <label for="SiteName">Url:</label>
            <input id="SiteName" class="inputText inputTextLarge" name="SiteName" type="text" />
        </li>
        <li>
            <label for="CommentText">Leave a comment</label>
            <textarea id="CommentText" rows="10" cols="30" name="CommentText"></textarea>
        </li>
        <li>
            <input type="submit" class="button" value="Submit Comment" />
        </li>
    </ol>
</div>
<% Html.EndForm(); %><%
    } %>