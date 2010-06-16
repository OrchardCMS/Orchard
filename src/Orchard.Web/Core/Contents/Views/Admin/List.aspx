<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<ListContentsViewModel>" %>
<%@ Import Namespace="Orchard.Core.Contents.ViewModels" %>
<% Html.AddTitleParts(T("Browse Contents").ToString()); %>
<p>
    Browse Contents</p>
<table>
    <% foreach (var t in Model.Entries) {%>
    <tr>
        <td>
            <%:t.ContentItem.Id %>.
        </td>
        <td>
            <%:t.ContentItem.ContentType %>
        </td>
        <td>
            ver #<%:t.ContentItem.Version %>
        </td>
        <td>
            <%if (t.ContentItemMetadata.DisplayRouteValues != null) {%>
            <%:Html.ActionLink(t.ContentItemMetadata.DisplayText, t.ContentItemMetadata.DisplayRouteValues["Action"].ToString(), t.ContentItemMetadata.DisplayRouteValues)%>
            <%}%>
        </td>
        <td>
            <%if (t.ContentItemMetadata.EditorRouteValues != null) {%>
            <%:Html.ActionLink("edit", t.ContentItemMetadata.EditorRouteValues["Action"].ToString(), t.ContentItemMetadata.EditorRouteValues)%>
            <%}%>
        </td>
    </tr>
    <%} %>
</table>
<p>
    <%:Html.ActionLink("Create new item", "Create", "Admin", new RouteValueDictionary{{"Area","Contents"},{"Id",Model.Id}}, new Dictionary<string, object>()) %></p>
