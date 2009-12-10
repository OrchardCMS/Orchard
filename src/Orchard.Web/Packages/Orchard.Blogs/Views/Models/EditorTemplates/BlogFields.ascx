<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Blog>" %>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models" %>
<fieldset>
    <label for="Name">Blog Name</label>
    <%=Html.EditorFor(m => m.Name) %>
</fieldset>
<fieldset>
    <label class="sub" for="permalink">Permalink: <span><%=Request.Url.ToRootString() %>/</span></label>
    <span><%=Html.TextBoxFor(m => m.Slug, new { id = "permalink", @class = "text" })%></span>
</fieldset>
<fieldset>
    <label for="Description">Description</label>
    <%=Html.TextAreaFor(m => m.Description, 5, 60, null) %>
</fieldset>