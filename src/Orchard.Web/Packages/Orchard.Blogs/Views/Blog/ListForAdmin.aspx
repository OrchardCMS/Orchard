<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogsViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<% Html.Include("AdminHead"); %>
    <h2>Manage Blogs</h2>
    <p>Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.</p>
    <%=Html.DisplayForModel() %>
<% Html.Include("AdminFoot"); %>