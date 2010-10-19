<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<IEnumerable<CommentPart>>" %>
<%@ Import Namespace="Orchard.Comments.Models"%>
<ul class="comments"><%
foreach (var comment in Model) { %>
    <li>
        <div class="comment">
            <p><%: comment.Record.CommentText %></p>
        </div>
        <div class="commentauthor">
            <span class="who"><%: Html.LinkOrDefault(Html.Encode(comment.Record.UserName), Html.Encode(comment.Record.SiteName), new { rel = "nofollow" })%></span>&nbsp;<span>said <%: Html.Link(Display.DateTimeRelative(dateTimeUtc: comment.Record.CommentDateUtc.GetValueOrDefault()).Text, "#")%></span>
        </div>       
    </li><%
} %>
</ul>
