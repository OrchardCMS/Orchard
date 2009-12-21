<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Blog>" %>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models" %>
<fieldset>
    <label for="Name">Blog Name</label>
    <%=Html.EditorFor(m => m.Name) %>
</fieldset>
<fieldset class="permalink">
    <label class="sub" for="Slug">Permalink: <span><%=Request.Url.ToRootString() %>/</span></label>
    <span><%=Html.TextBoxFor(m => m.Slug, new { @class = "text" })%></span>
</fieldset>
<fieldset>
    <label for="Description">Description</label>
    <%=Html.TextAreaFor(m => m.Description, 5, 60, null) %>
</fieldset>