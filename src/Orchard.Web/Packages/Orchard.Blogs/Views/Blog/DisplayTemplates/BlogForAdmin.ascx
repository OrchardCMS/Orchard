<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Blog>" %>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<h3><a href="<%=Url.BlogForAdmin(Model.Slug) %>"><%=Html.Encode(Model.Name) %></a></h3>
<div class="meta"><a href="<%=Url.BlogForAdmin(Model.Slug) %>"><%=Model.PostCount %> posts</a> | <a href="#"><%=(new Random()).Next(0, 1000) %> comments</a></div>
<%--<p>[list of authors] [modify blog access]</p>--%>
<p><%=Model.Description %></p>
<p class="actions">
    <%-- todo: (heskew) make into a ul --%>
    <span class="construct">
        <a href="<%=Url.BlogEdit(Model.Slug) %>" class="ibutton edit" title="Edit Blog">Edit Blog</a>
        <a href="<%=Url.Blog(Model.Slug) %>" class="ibutton view" title="View Blog">View Blog</a>
        <a href="<%=Url.BlogPostCreate(Model.Slug) %>" class="ibutton add page" title="Add Post">Add Post</a>
    </span>
    <span class="destruct"><a href="<%=Url.BlogDelete(Model.Slug) %>" class="ibutton remove" title="Delete Blog">Remove Blog</a></span>
</p>