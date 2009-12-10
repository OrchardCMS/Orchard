<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BlogPost>" %>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<fieldset>
    <label for="title">Title</label>
    <span><%=Html.TextBoxFor(m => m.Title, new { id = "title", @class = "large text" })%></span>
</fieldset>
<fieldset>
    <label class="sub" for="permalink">Permalink<br /><span><%=Request.Url.ToRootString() %>/<%=Model.Blog.Slug %>/</span></label>
    <span><%=Html.TextBoxFor(m => m.Slug, new { id = "permalink", @class = "text" })%></span>
</fieldset>