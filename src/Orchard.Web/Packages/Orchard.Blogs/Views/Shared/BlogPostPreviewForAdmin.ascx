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
        <a href="<%=Url.BlogPostEdit(Model.Blog.Slug, Model.As<RoutableAspect>().Slug) %>">edit</a>
        | <a href="<%=Url.BlogPost(Model.Blog.Slug, Model.As<RoutableAspect>().Slug) %>">view</a>
    </span>
    <span class="destruct"><a href="#">delete</a></span>
</p>