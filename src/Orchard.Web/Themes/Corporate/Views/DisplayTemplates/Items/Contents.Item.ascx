<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel>" %>
<%@ Import Namespace="Orchard.Security"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<div class="page-title"><%:Html.ItemDisplayText(Model.Item) %></div>
<%--<% if (Html.Resolve<IAuthenticationService>().GetAuthenticatedUser() != null){ %>
<%} %>--%>

<% Html.Zone("primary");
   Html.ZonesAny(); %>
   
<%--<% if (Html.Resolve<IAuthenticationService>().GetAuthenticatedUser() != null){ %>
</a>
<%} %>--%>