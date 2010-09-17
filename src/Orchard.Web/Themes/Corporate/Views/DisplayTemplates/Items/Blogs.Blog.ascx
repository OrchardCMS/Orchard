<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<BlogPart>>" %>
<%@ Import Namespace="Orchard.Security"%>
<%@ Import Namespace="Orchard.UI.Resources"%>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<%-- todo: (heskew) selectively display to those who have access --%>
<h1 class="page-title"><%: Html.TitleForPage(Model.Item.Name) %></h1>

<% RegisterLink(new LinkEntry { Rel = "wlwmanifest", Type = "application/wlwmanifest+xml", Href = Url.BlogLiveWriterManifest(Model.Item.Slug) });%>
<% RegisterLink(new LinkEntry { Rel = "EditURI", Type = "application/rsd+xml", Title = "RSD", Href = Url.BlogRsd(Model.Item.Slug) });%>

<% Html.Zone("primary"); %>
