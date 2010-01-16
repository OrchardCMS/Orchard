<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<IEnumerable<Comment>>" %>
<%@ Import Namespace="Orchard.Comments.Models"%>
<ul class="comments"><%
foreach (var comment in Model) { %>
    <li>

<div class="comment">
<p><%=Html.Encode(comment.CommentText) %></p>
</div>
        
<div class="commentauthor">
<span class="who"><%=Html.LinkOrDefault(Html.Encode(comment.UserName), Html.Encode(comment.SiteName), new { rel = "nofollow" })%></span>&nbsp;<span>said <%=Html.Link(Html.DateTimeRelative(comment.CommentDate), "#")%></span>
</div>       
        
    </li><%
} %>
</ul>
