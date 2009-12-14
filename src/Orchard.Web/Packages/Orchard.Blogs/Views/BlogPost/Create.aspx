<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<CreateBlogPostViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<% Html.Include("AdminHead"); %>
    <h2>Add Post</h2>
    <% using (Html.BeginForm()) { %>
        <%=Html.ValidationSummary() %>
        <%=Html.EditorForItem(m => m.BlogPost) %>
        <%=Html.OrchardAntiForgeryToken() %><%
       } %>
<% Html.Include("AdminFoot"); %>