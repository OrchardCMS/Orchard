<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ShowDebugLink>" %>
<%@ Import Namespace="Orchard.DevTools.Models" %>
<div>
    DevTools: editing
    <%= Html.ActionLink(Model.ContentItem.ContentType + " #" + Model.ContentItem.Id, "details", "content", new { area = "Orchard.DevTools", Model.ContentItem.Id }, new { })%></div>
