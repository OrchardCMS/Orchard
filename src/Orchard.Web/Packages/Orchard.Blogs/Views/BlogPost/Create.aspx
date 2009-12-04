<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<CreateBlogPostViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Security" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<% Html.Include("AdminHead"); %>
    <h2>Add Post</h2>
    <%using (Html.BeginForm()) { %>
        <%= Html.ValidationSummary() %>
        <%= Html.EditorForModel() %>
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