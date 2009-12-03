<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<CreateBlogPostViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Security" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<% Html.Include("AdminHead"); %>
    <h2>Create a New Blog Post</h2>
    <p><a href="<%=Url.BlogsForAdmin() %>">Manage Blogs</a> &gt; <a href="<%=Url.BlogEdit(Model.Blog.Slug) %>"><%=Html.Encode(Model.Blog.Name) %></a> &gt; Create Blog Post</p>
    <%using (Html.BeginForm()) { %>
        <%= Html.ValidationSummary() %>
        <%= Html.EditorForModel() %>
    <% } %>
<% Html.Include("AdminFoot"); %>