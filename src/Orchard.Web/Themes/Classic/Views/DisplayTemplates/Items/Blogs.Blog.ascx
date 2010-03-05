<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<Blog>>" %>
<%@ Import Namespace="Orchard.UI.Resources"%>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<h1><%=Html.TitleForPage(Model.Item.Name) %></h1>
<% Html.RegisterLink(new LinkEntry { Rel = "wlwmanifest", Type = "application/wlwmanifest+xml", Href = Url.BlogLiveWriterManifest(Model.Item.Slug) });%>
<% Html.RegisterLink(new LinkEntry { Rel = "EditURI", Type = "application/rsd+xml", Title = "RSD", Href = Url.BlogRsd(Model.Item.Slug) });%>
<% Html.Zone("primary", ":manage");%>