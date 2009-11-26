<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.Blogs.ViewModels.CreateBlogViewModel>" ValidateRequest="false" %>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Security" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<% Html.Include("Head"); %>
    <h2>Create New Blog</h2>
    <p><a href="<%=Url.Blogs() %>">Manage Blogs</a> &gt; Create Blog</p>
    <% using (Html.BeginForm()) { %>
        <%=Html.ValidationSummary() %>
        <%=Html.EditorForModel() %>
        <fieldset><input class="button" type="submit" value="Create" /></fieldset>
    <% } %>
<% Html.Include("Foot"); %>