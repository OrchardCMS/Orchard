<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<BlogPost>" %>
<%@ Import Namespace="Orchard.Blogs"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%><%
if (AuthorizedFor(Permissions.EditOthersBlogPost)) { %>
<div class="manage">
    <a href="<%=Url.BlogPostEdit(Model.Blog.Slug, Model.Id) %>" class="edit"><%=_Encoded("Edit") %></a>
</div><%
} %>