<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BlogForAdminViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<% Html.Title("Manage Blog"); %>
<%=Html.DisplayForItem(m => m.Blog) %>