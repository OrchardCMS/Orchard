<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BlogPost>" %>
<%@ Import Namespace="Orchard.Models"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Core.Common.Models"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<h3><a href="<%=Url.BlogPostEdit(Model.Blog.Slug, Model.As<RoutableAspect>().Slug) %>"><%=Html.Encode(Model.As<RoutableAspect>().Title) %></a></h3>
<div class="meta"><%=Html.Published() %></div>
<div class="content"><%=Model.Body ?? "<p><em>there's no content for this blog post</em></p>" %></div>
<p class="actions">
    <%-- todo: (heskew) make into a ul --%>
    <span class="construct">
        <a href="<%=Url.BlogPostEdit(Model.Blog.Slug, Model.As<RoutableAspect>().Slug) %>">Edit</a>
        | <a href="<%=Url.BlogPost(Model.Blog.Slug, Model.As<RoutableAspect>().Slug) %>">View</a><%
        if (Model.Published == null) { // todo: (heskew) be smart about this and maybe have other contextual actions %>
        | <a href="<%=Url.BlogPost(Model.Blog.Slug, Model.As<RoutableAspect>().Slug) %>">Publish</a>
        <% } %>
    </span>
    <span class="destruct"><a href="#">Delete Blog Post</a></span>
</p>