<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<Simple>" %>

<%@ Import Namespace="Orchard.DevTools.Models" %>
<h1>
    <%= H(Model.Title) %></h1>
<p>
    Quantity:
    <%= Model.Quantity %></p>
<div style="border: solid 1px #ccc;">
    <% Html.RenderAction("_RenderableAction"); %></div>
<p>
    <%: Html.ActionLink("Test Messages", "SimpleMessage")%></p>
