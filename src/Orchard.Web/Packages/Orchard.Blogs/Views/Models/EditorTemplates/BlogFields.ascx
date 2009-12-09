<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Blog>" %>
<%@ Import Namespace="Orchard.Blogs.Models" %>
<fieldset>
    <label for="Name">Blog Name</label>
    <%=Html.EditorFor(m => m.Name) %>
</fieldset>
<%=Html.EditorFor(m => m.Slug, "BlogPermalink") %>
<fieldset>
    <label for="Description">Description</label>
    <%=Html.TextAreaFor(m => m.Description, 5, 60, null) %>
</fieldset>