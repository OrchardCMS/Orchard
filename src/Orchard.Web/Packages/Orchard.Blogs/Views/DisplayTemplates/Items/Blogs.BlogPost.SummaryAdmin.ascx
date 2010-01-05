<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ContentItemViewModel<BlogPost>>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.ContentManagement"%>
<%@ Import Namespace="Orchard.Core.Common.Models"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<h3><a href="<%=Url.BlogPostEdit(Model.Item.Blog.Slug, Model.Item.Slug) %>"><%=Html.Encode(Model.Item.Title)%></a></h3>
<div class="meta">
    <%=Html.PublishedState(Model.Item) %>
    | <a href="#">?? comments</a>
</div>
<div class="content"><%=Model.Item.As<BodyAspect>().Text ?? "<p><em>there's no content for this blog post</em></p>"%></div>
<p class="actions">
    <%-- todo: (heskew) make into a ul --%>
    <span class="construct">
        <a href="<%=Url.BlogPostEdit(Model.Item.Blog.Slug, Model.Item.Slug) %>" class="ibutton edit" title="Edit Post">Edit Post</a>
        <a href="<%=Url.BlogPost(Model.Item.Blog.Slug, Model.Item.Slug) %>" class="ibutton view" title="View Post">View Post</a><%
        if (Model.Item.Published == null) { // todo: (heskew) be smart about this and maybe have other contextual actions - including view/preview for view up there ^^ %>
        <a href="<%=Url.BlogPost(Model.Item.Blog.Slug, Model.Item.Slug) %>" class="ibutton publish" title="Publish Post Now">Publish Post Now</a>
        <% } %>
    </span>
    <span class="destruct"><a href="#" class="ibutton remove" title="Remove Post">Remove Post</a></span>
</p>