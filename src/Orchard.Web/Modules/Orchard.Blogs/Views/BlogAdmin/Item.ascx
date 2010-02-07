<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<BlogForAdminViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<% Html.AddTitleParts(T("Manage Blog").ToString()); %>
<%=Html.DisplayForItem(m => m.Blog) %>