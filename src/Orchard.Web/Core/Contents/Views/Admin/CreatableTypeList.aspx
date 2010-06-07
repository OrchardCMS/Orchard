<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<ContentTypeListViewModel>" %>

<%@ Import Namespace="Orchard.Core.Contents.ViewModels" %>
<% Html.AddTitleParts(T("Create Content").ToString()); %>
<p>
    Create content</p>
<ul>
    <% foreach (var t in Model.Types) {%>
    <li>
        <%:Html.ActionLink(t.Name, "Create", new RouteValueDictionary{{"Area","Contents"},{"Id",t.Name}}) %></li>
    <%} %>
</ul>
