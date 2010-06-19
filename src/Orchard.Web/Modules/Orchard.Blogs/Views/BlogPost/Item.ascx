<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BlogPostViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<% Html.AddTitleParts(Model.Blog.Name); %>
<%: Html.DisplayForItem(m => m.BlogPost) %>
