<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels" %>
<h1 class="page-title"><%:Html.ItemDisplayText(Model.Item)%></h1>
<% Html.Zone("metadata");
   Html.Zone("primary", ":manage :metadata");
   Html.ZonesAny(); %>
