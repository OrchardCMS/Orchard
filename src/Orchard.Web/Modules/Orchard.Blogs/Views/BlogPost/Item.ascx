<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BlogPostViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<% Html.AddTitleParts(Model.BlogPart.Name); %>
<%: Html.DisplayForItem(m => m.BlogPost) %>
