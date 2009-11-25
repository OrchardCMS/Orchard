<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BlogsViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<%@ Import Namespace="Orchard.Blogs.ViewModels"
%><div class="actions"><a class="add button" href="<%=Url.BlogCreate() %>">Create a New Blog</a></div><%
if (Model.Blogs.Count() > 0) { %>
<ul><%
    foreach (Blog blog in Model.Blogs) { %>
    <li>
        <h3><a href="<%=Url.BlogEdit(blog.Slug) %>"><%=Html.Encode(blog.Name) %></a> <span>(<a href="<%=Url.Blog(blog.Slug) %>">view</a>)</span> <span>(<a href="<%=Url.BlogPostCreate(blog.Slug) %>">post</a>)</span></h3>
        <p>[list of authors] [modify blog access]</p>
        <p><%=blog.Description %></p>
    </li><%
    } %>
</ul>
<div class="actions"><a class="add button" href="<%=Url.BlogCreate() %>">Create a New Blog</a></div><%
} %>