<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BlogViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<%=Html.DisplayForItem(m => m.Blog) %>