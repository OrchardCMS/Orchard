<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<BlogPart>" %>
<%@ Import Namespace="Orchard.Blogs"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%><%
Html.RegisterStyle("admin.css"); 
if (AuthorizedFor(Permissions.ManageBlogs)) { %>
<div class="item-properties actions">
    <p><a href="<%: Url.BlogEdit(Model.Slug) %>" class="edit"><%: T("Blog Properties") %></a></p>
</div><%
} %>