<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BlogPost>" %>
<%@ Import Namespace="Orchard.Models"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Core.Common.Models"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<h3><a href="<%=Url.BlogPostEdit(Model.Blog.Slug, Model.Slug) %>"><%=Html.Encode(Model.Title) %></a></h3>
<div class="meta"><%=Html.Published() %></div>
<div class="content"><%=Model.Body ?? "<p><em>there's no content for this blog post</em></p>" %></div>
<p class="actions">
    <%-- todo: (heskew) make into a ul --%>
    <span class="construct">
        <a href="<%=Url.BlogPostEdit(Model.Blog.Slug, Model.Slug) %>">Edit</a>
        | <a href="<%=Url.BlogPost(Model.Blog.Slug, Model.Slug) %>">View</a><%
        if (Model.Published == null) { // todo: (heskew) be smart about this and maybe have other contextual actions - including view/preview for view up there ^^ %>
        | <a href="<%=Url.BlogPost(Model.Blog.Slug, Model.Slug) %>">Publish Now</a>
        <% } %>
    </span>
    <span class="destruct"><a href="#">Delete Post</a></span>
</p>