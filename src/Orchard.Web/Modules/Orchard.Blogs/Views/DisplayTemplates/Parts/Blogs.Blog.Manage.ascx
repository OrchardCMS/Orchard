<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Blog>" %>
<%@ Import Namespace="Orchard.Blogs"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%><%
Html.RegisterStyle("admin.css"); 
if (AuthorizedFor(Permissions.ManageBlogs)) { %>
<div class="folderProperties">
    <p><a href="<%: Url.BlogEdit(Model.Slug) %>" class="edit"><%: T("Edit") %></a></p>
</div><%
} %>