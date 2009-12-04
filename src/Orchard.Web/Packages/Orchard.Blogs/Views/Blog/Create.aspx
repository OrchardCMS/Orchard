<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.Blogs.ViewModels.CreateBlogViewModel>" ValidateRequest="false" %>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Security" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<% Html.Include("AdminHead"); %>
    <h2>Add Blog</h2>
    <% using (Html.BeginForm()) { %>
        <%=Html.ValidationSummary() %>
        <%=Html.EditorForModel() %>
        <fieldset><input class="button" type="submit" value="Create" /></fieldset>
    <% } %>
<% Html.Include("AdminFoot"); %>