<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<HasTags>" %>
<%@ Import Namespace="Orchard.Tags.Models" %>
<% if (Model.CurrentTags.Count > 0) { %>
    <p class="tags">
        <span><%=_Encoded("Tags:") %></span>
        <%=string.Join(", ", Model.CurrentTags.Select(t => Html.ActionLink(Html.Encode(t.TagName), "Search", "Home", new { area = "Orchard.Tags", tagName = t.TagName }, new { }).ToHtmlString()).ToArray())%>
    </p><%
} %>