<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogEditViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<% Html.Include("Head"); %>
    <h2>Edit Blog</h2>
    <p><a href="<%=Url.Blogs() %>">Manage Blogs</a> &gt; Editing <strong><%=Html.Encode(Model.Name)%></strong></p>
    <% using (Html.BeginForm()) { %>
        <%=Html.ValidationSummary() %>
        <%=Html.EditorForModel() %>
        <fieldset><input class="button" type="submit" value="Save" /></fieldset>
    <% } %>
<% Html.Include("Foot"); %>