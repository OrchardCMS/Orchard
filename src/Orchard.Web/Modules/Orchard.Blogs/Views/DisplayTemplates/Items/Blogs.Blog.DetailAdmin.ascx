<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<BlogPart>>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<h1><a href="<%: Url.BlogForAdmin(Model.Item.Slug) %>"><%: Html.TitleForPage(Model.Item.Name) %></a></h1>
<% Html.Zone("manage"); %>
<div class="manage"><a href="<%: Url.BlogPostCreate(Model.Item) %>" class="add button primaryAction"><%: T("New Post")%></a></div>
<% Html.Zone("primary"); %>