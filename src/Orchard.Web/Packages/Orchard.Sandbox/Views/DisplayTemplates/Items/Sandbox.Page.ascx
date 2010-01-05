<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ItemViewModel<SandboxPage>>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Sandbox.Models" %>
<div class="item">
    <% Html.Zone("first"); %>
    <div class="title">
        <% Html.Zone("title"); %>
    </div>
    <% Html.Zone("metatop"); %>
    <div class="actions">
        <%=Html.ItemEditLink("Edit this page", Model.Item) %>
        <%=Html.ActionLink("Return to list", "index") %>
        <% Html.Zone("actions"); %>
    </div>
    <div class="body">
        <% Html.Zone("body"); %>
    </div>
    <% Html.Zone("metabottom"); %>
    <div class="footer">
        <% Html.ZonesExcept("last"); %>
        <% Html.Zone("last"); %>
    </div>
</div>
