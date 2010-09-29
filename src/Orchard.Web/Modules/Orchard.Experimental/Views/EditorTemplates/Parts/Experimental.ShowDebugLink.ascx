<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ShowDebugLink>" %>
<%@ Import Namespace="Orchard.Experimental.Models" %>
<% if (Model.ContentItem.Id > 0) { %>
<div class="debug message"><%=T(
    "DevTools: editing {0}",
    Html.ActionLink(T("{0} #{1} v{2}", Model.ContentItem.ContentType, Model.ContentItem.Id, Model.ContentItem.Version).ToString(), "details", "content", new { area = "Orchard.Experimental", Model.ContentItem.Id, Model.ContentItem.Version }, new { })
    ) %></div>
<% } %>
