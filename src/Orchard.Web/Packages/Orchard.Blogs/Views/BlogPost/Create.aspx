<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<CreateBlogPostViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Security" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<% Html.Include("AdminHead"); %>
    <h2>Add Post</h2>
    <%using (Html.BeginForm()) { %>
        <%= Html.ValidationSummary() %>
        <%= Html.EditorForItem(m => m.BlogPost) %>
    <% } %>
<% Html.Include("AdminFoot"); %>