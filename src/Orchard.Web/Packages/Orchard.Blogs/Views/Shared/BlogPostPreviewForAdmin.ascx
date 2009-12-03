<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BlogPost>" %>
<%@ Import Namespace="Orchard.Models"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Core.Common.Models"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<h3><a href="<%=Url.BlogPostEdit(Model.Blog.Slug, Model.As<RoutableAspect>().Slug) %>"><%=Html.Encode(Model.As<RoutableAspect>().Title) %></a></h3>
<div class="posted"><%=Html.Published() %></div>
<div class="content"><%=Model.Body %></div>