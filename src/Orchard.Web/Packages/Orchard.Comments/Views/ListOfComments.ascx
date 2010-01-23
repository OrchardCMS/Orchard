<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<IEnumerable<Comment>>" %>
<%@ Import Namespace="Orchard.Comments.Models"%>
<ul class="comments"><%
foreach (var comment in Model) { %>
    <li>
        <div class="comment">
            <span class="who"><%=Html.LinkOrDefault(Html.Encode(comment.Record.UserName), Html.Encode(comment.Record.SiteName), new { rel = "nofollow" })%></span>
            <%-- todo: (heskew) need comment permalink --%>
            <span>said <%=Html.Link(Html.DateTimeRelative(comment.Record.CommentDateUtc), "#")%></span>
        </div>
        <div class="text">
            <%-- todo: (heskew) comment text needs processing depending on comment markup style --%>
            <p><%=Html.Encode(comment.Record.CommentText)%></p>
        </div>
    </li><%
} %>
</ul>