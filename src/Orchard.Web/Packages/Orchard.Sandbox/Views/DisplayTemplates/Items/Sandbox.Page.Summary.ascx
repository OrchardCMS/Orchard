<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<SandboxPage>>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Sandbox.Models" %>
<div class="item">
<% Html.Zone("title");
   Html.Zone("metatop");
   Html.Zone("body"); %>
</div>