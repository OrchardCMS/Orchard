<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels" %>
<h1><%:Html.ItemDisplayText(Model.Item)%></h1>
<% Html.Zone("metadata");
   Html.Zone("primary", ":manage :metadata");
   Html.ZonesAny(); %>
