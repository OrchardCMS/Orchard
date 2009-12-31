<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ShowDebugLink>" %>
<%@ Import Namespace="Orchard.DevTools.Models" %>
<div class="debug message">
    DevTools: showing
    <%= Html.ActionLink(Model.ContentItem.ContentType + " #" + Model.ContentItem.Id + " v" + Model.ContentItem.Version, "details", "content", new { area = "Orchard.DevTools", Model.ContentItem.Id, Model.ContentItem.Version }, new { })%></div>
