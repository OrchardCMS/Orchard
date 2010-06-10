<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels" %>
<%@ Import Namespace="Orchard.ContentManagement.Aspects" %>
<%@ Import Namespace="Orchard.ContentManagement" %>
<%var routable = Model.Item.As<IRoutableAspect>();
  if (routable != null && !string.IsNullOrEmpty(routable.Title)) {%>
<h1>
    <%:routable.Title%></h1>
<%} %>
<% Html.Zone("primary", ":manage :metadata");
   Html.ZonesAny(); %>
