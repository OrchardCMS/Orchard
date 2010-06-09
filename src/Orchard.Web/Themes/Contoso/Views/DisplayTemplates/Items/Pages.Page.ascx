<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<Orchard.Pages.Models.Page>>" %>
<%@ Import Namespace="Orchard.Security"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>

<div class="page-title"><%: Html.TitleForPage(Model.Item.Title)%></div>

<%--<% if (Html.Resolve<IAuthenticationService>().GetAuthenticatedUser() != null){ %>
<%} %>--%>

<% Html.Zone("primary");
   Html.ZonesAny(); %>
   
<%--<% if (Html.Resolve<IAuthenticationService>().GetAuthenticatedUser() != null){ %>
</a>
<%} %>--%>