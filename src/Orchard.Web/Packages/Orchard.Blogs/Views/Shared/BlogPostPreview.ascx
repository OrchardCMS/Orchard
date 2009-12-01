<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BlogPost>" %>
<%@ Import Namespace="Orchard.Models"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Core.Common.Models"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<h2 class="title"><a href="<%=Url.BlogPost(Model.Blog.Slug, Model.As<RoutableAspect>().Slug) %>"><%=Html.Encode(Model.As<RoutableAspect>().Title) %></a></h2>
<div class="posted"><%=Html.Published() %></div>
<div class="content"><%=Model.Body %></div>