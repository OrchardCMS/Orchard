<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Blog>" %>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<h3><a href="<%=Url.BlogForAdmin(Model.Slug) %>"><%=Html.Encode(Model.Name) %></a></h3>
<div class="meta"><%=Model.PostCount %> posts</div>
<%--<p>[list of authors] [modify blog access]</p>--%>
<p><%=Model.Description %></p>
<p class="actions">
    <%-- todo: (heskew) make into a ul --%>
    <span class="construct">
        <a href="<%=Url.BlogForAdmin(Model.Slug) %>">All Posts</a>
        | <a href="<%=Url.BlogEdit(Model.Slug) %>">Edit</a>
        | <a href="<%=Url.Blog(Model.Slug) %>">View</a>
        | <a href="<%=Url.BlogPostCreate(Model.Slug) %>">Create a Blog Post</a>
    </span>
    <span class="destruct"><a href="#">Delete Blog</a></span>
</p>