<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.Blogs.ViewModels.CreateBlogViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Security" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<% Html.Include("Head"); %>
    <h2>Create New Blog</h2>
    <p><a href="<%=Url.Blogs() %>">Manage Blogs</a> > Create</p>
    <%using (Html.BeginForm()) { %>
        <%= Html.ValidationSummary() %>
        <%= Html.EditorForModel() %>
    <%}/*EndForm*/%>
<% Html.Include("Foot"); %>