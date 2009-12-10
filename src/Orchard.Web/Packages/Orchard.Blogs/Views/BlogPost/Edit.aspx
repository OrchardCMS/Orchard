<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogPostEditViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<% Html.Include("AdminHead"); %>
    <h2>Edit Post</h2>
    <% using (Html.BeginForm()) { %>
        <%=Html.ValidationSummary() %>
        <%=Html.EditorForItem(m => m.BlogPost) %>
    <% } %>
<% Html.Include("AdminFoot"); %>