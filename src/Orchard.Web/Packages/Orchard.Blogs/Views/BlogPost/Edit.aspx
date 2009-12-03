<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogPostEditViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<% Html.Include("AdminHead"); %>
    <h2>Edit Blog Post</h2>
    <p><a href="<%=Url.BlogsForAdmin() %>">Manage Blogs</a> &gt; <a href="<%=Url.BlogForAdmin(Model.Blog.Slug) %>"><%=Html.Encode(Model.Blog.Name)%></a> &gt; <a href="<%=Url.BlogPostEdit(Model.Blog.Slug, Model.Post.Slug) %>"><%=Model.Title %></a></p>
    <% using (Html.BeginForm()) { %>
        <%=Html.ValidationSummary() %>
        <%=Html.EditorForModel() %>
        <fieldset><input class="button" type="submit" value="Save" /></fieldset>
        <%foreach (var editor in Model.ItemView.Editors) { %>
        <%-- TODO: why is Body in editors? --%>
        <%-- TODO: because any content type using the body editor doesn't need
        to re-implement the rich editor, media extensions, format filter chain selection, etc --%>
            <% if (!String.Equals(editor.Prefix, "Body")) { %>
                <%=Html.EditorFor(m=>editor.Model, editor.TemplateName, editor.Prefix) %>
            <% } %>
        <%} %>
    <% } %>
<% Html.Include("AdminFoot"); %>