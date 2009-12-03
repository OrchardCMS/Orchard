<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ItemDisplayViewModel<SandboxPage>>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Sandbox.Models" %>
<%@ Import Namespace="Orchard.Models.ViewModels" %>
<%@ Import Namespace="Orchard.Models" %>

<h1><%=Html.Encode(Model.Item.Record.Name) %></h1>

<%foreach (var display in Model.Displays) { %>
<%=Html.DisplayFor(m=>display.Model, display.TemplateName, display.Prefix) %>
<%} %>

            <p>
                <%=Html.ItemEditLink("Edit this page", Model.Item) %>, <%=Html.ActionLink("Return to list", "index") %></p>
