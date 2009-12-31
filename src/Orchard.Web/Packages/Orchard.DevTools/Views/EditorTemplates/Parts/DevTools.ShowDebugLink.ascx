<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ShowDebugLink>" %>
<%@ Import Namespace="Orchard.DevTools.Models" %>
<% if (Model.ContentItem.Id > 0) { %>
<div class="debug message">
    DevTools: editing
    <%= Html.ActionLink(Model.ContentItem.ContentType + " #" + Model.ContentItem.Id + " v" + Model.ContentItem.Version, "details", "content", new { area = "Orchard.DevTools", Model.ContentItem.Id, Model.ContentItem.Version }, new { })%></div>
<% } %>
