<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BlogPost>" %>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<fieldset>
    <label for="Title">Title</label>
    <span><%=Html.TextBoxFor(m => m.Title, new { @class = "large text" })%></span>
</fieldset>
<fieldset class="permalink">
    <label class="sub" for="Slug">Permalink<br /><span><%=Request.Url.ToRootString() %>/<%=Model.Blog.Slug %>/</span></label>
    <span><%=Html.TextBoxFor(m => m.Slug, new { @class = "text" })%></span>
</fieldset>