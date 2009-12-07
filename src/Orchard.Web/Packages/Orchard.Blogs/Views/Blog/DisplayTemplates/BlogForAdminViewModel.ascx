<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BlogForAdminViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<%-- todo: (heskew) get what actions we can out of the h2 :| --%>
<h2 class="withActions">
    <a href="<%=Url.BlogForAdmin(Model.Blog.Slug) %>"><%=Html.Encode(Model.Blog.Name) %></a>
    <a href="<%=Url.BlogEdit(Model.Blog.Slug) %>" class="ibutton edit" title="Edit Blog">Edit Blog</a>
    <span class="actions"><span class="destruct"><a href="<%=Url.BlogDelete(Model.Blog.Slug) %>" class="ibutton remove" title="Remove Blog">Remove Blog</a></span></span></h2>
<p><%=Model.Blog.Description %></p><%
if (Model.Posts.Count() > 0) { %>
<div class="actions"><a href="<%=Url.BlogPostCreate(Model.Blog.Slug) %>" class="add button">New Post</a></div>
<%=Html.UnorderedList(Model.Posts, (p, i) => Html.DisplayFor(blog => p, "BlogPostPreviewForAdmin").ToHtmlString(), "contentItems")%>
<div class="actions"><a href="<%=Url.BlogPostCreate(Model.Blog.Slug) %>" class="add button">New Post</a></div><%
} else { %>
<div class="info message">This blog is sad with no posts, but don't fret. You can add a new post right <a href="<%=Url.BlogPostCreate(Model.Blog.Slug) %>">here</a>!</div><%
} %>