<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<CreateBlogViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Security" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<% Html.Include("AdminHead"); %>
    <h2>Add Blog</h2>
    <% using (Html.BeginForm()) { %>
        <%=Html.ValidationSummary() %>
        <%=Html.EditorForItem(vm => vm.Blog) %>
        <fieldset><input class="button" type="submit" value="Create" /></fieldset>
    <% } %>
<% Html.Include("AdminFoot"); %>