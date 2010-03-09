<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<Blog>>" %>
<%@ Import Namespace="Orchard.Security"%>
<%@ Import Namespace="Orchard.UI.Resources"%>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<%-- todo: (heskew) selectively display to those who have access --%>
<h1 class="page-title"><%=Html.TitleForPage(Model.Item.Name) %></h1>
<% Html.RegisterLink(new LinkEntry { Rel = "wlwmanifest", Type = "application/wlwmanifest+xml", Href = Url.BlogLiveWriterManifest(Model.Item.Slug) });%>
<% Html.RegisterLink(new LinkEntry { Rel = "EditURI", Type = "application/rsd+xml", Title = "RSD", Href = Url.BlogRsd(Model.Item.Slug) });%>

<% if (Html.Resolve<IAuthenticationService>().GetAuthenticatedUser() != null){ %>
<div class="manage"><a href="<%=Url.BlogEdit(Model.Item.Slug) %>" class="ibutton edit"><%=_Encoded("edit") %></a></div>
<%} %>

<p class="blog-desc"><%=Html.Encode(Model.Item.Description) %></p>

<% Html.Zone("primary");
   Html.ZonesAny(); %>
   
