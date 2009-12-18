<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BlogPostViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<% Html.Title(Model.Blog.Name); %>
<%=Html.DisplayForItem(m=>m.BlogPost) %>