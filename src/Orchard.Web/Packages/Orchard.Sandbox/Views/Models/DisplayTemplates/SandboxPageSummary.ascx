<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ItemDisplayViewModel<SandboxPage>>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Sandbox.Models" %>
<%@ Import Namespace="Orchard.Models.ViewModels" %>
<%@ Import Namespace="Orchard.Models" %>
<h2><%=Html.ItemDisplayLink(Model.Item) %></h2>
<%--<%foreach (var display in Model.Displays) { %>
<%=Html.DisplayFor(m=>display.Model, display.TemplateName, display.Prefix??"") %>
<%} %>
--%>