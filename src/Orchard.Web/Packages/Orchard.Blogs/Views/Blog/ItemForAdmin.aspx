<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogForAdminViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<% Html.Include("AdminHead"); %>
    <%=Html.DisplayForItem(m => m.Blog) %>
<% Html.Include("AdminFoot"); %>