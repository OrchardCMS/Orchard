<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ItemDisplayModel<BlogPost>>" %>
<%@ Import Namespace="Orchard.Models"%>
<%@ Import Namespace="Orchard.Core.Common.Models"%>
<%@ Import Namespace="Orchard.Models.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<h3><a href="<%=Url.BlogPost(Model.Item.Blog.Slug, Model.Item.Slug) %>"><%=Html.Encode(Model.Item.Title) %></a></h3>
<div class="meta">
    <%=Html.PublishedState(Model.Item) %>
    | <a href="#">?? comments</a>
</div>
<div class="content"><%=Model.Item.As<BodyAspect>().Text ?? "<p><em>there's no content for this blog post</em></p>" %></div>