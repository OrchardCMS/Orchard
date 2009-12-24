<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ItemDisplayModel<SandboxPage>>" %>
<%@ Import Namespace="Orchard.Sandbox.Models" %>
<%@ Import Namespace="Orchard.ContentManagement.ViewModels" %>

<div class="item">
<% Html.Zone("title"); %>
<% Html.Zone("metatop"); %>
<% Html.Zone("body"); %>
</div>
