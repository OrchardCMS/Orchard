<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<SandboxPage>>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Sandbox.Models" %>
<div class="item">
    <% Html.Zone("first"); %>
    <div class="title">
        <% Html.Zone("title"); %>
    </div>
    <% Html.Zone("metatop"); %>
    <div class="actions">
        <%: Html.ItemEditLink(T("Edit this page").ToString(), Model.Item) %>
        <%: Html.ActionLink(T("Return to list").ToString(), "index") %>
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