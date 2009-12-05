<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BlogPost>" %>
<%@ Import Namespace="Orchard.Models"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Core.Common.Models"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<h3><a href="<%=Url.BlogPost(Model.Blog.Slug, Model.As<RoutableAspect>().Slug) %>"><%=Html.Encode(Model.As<RoutableAspect>().Title) %></a></h3>
<div class="blog metadata"><%=Html.PublishedState() %> | <a href="<%=Url.BlogPost(Model.Blog.Slug, Model.As<RoutableAspect>().Slug) %>#comments">?? comments</a></div>
<div class="content"><%=Model.Body %></div>