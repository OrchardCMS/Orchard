<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<HasTags>" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<%@ Import Namespace="Orchard.Tags.Models" %>
<ul class="tags">
    <%foreach (var tag in Model.CurrentTags) {%>
    <li class="tag"><%=Html.ActionLink(tag.TagName, "Search", "Home", new{ area="Orchard.Tags", tagName=tag.TagName},new{}) %></li>
    <%}%>
</ul>
