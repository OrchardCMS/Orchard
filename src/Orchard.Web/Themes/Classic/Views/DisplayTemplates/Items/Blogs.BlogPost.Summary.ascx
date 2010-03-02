<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<BlogPost>>" %>
<%@ Import Namespace="Orchard.ContentManagement"%>
<%@ Import Namespace="Orchard.Core.Common.Models"%>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<h2><%=Html.Link(Html.Encode(Model.Item.Title), Url.BlogPost(Model.Item.Blog.Slug, Model.Item.Slug)) %></h2>
<div class="meta"><%=Html.PublishedState(Model.Item) %> | <%Html.Zone("meta");%></div>
<div class="postsummary"><%=Model.Item.Text ?? string.Format("<p><em>{0}</em></p>", _Encoded("there's no content for this blog post"))%></div>
