<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<ContentTypeListViewModel>" %>

<%@ Import Namespace="Orchard.Core.Contents.ViewModels" %>
<% Html.AddTitleParts(T("Create Content").ToString()); %>
<p>
    Create content</p>
<table>
    <% foreach (var t in Model.Types) {%>
    <tr>
        <td>
            <%:t.Name %>
        </td>
        <td>
            <%:Html.ActionLink(T("List Items"), "List", "Admin", new RouteValueDictionary{{"Area","Contents"},{"Id",t.Name}}, new Dictionary<string, object>()) %>
        </td>
        <td>
            <%:Html.ActionLink(T("Create Item"), "Create", "Admin", new RouteValueDictionary{{"Area","Contents"},{"Id",t.Name}}, new Dictionary<string, object>()) %>
        </td>
        <td>
            <%:Html.ActionLink(T("Edit Type"), "ContentTypeList", "Admin", new RouteValueDictionary{{"Area","Orchard.MetaData"},{"Id",t.Name}}, new Dictionary<string, object>()) %>
        </td>
    </tr>
    <%} %>
</table>
