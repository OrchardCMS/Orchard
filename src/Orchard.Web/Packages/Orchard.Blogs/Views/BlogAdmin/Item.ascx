<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BlogForAdminViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<% Html.AddTitleParts("Manage Blog"); %>
<%=Html.DisplayForItem(m => m.Blog) %>