<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<BlogPost>>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<h3><%=Html.Link(Html.Encode(Model.Item.Title), Url.BlogPost(Model.Item)) %></h3>
<div class="meta"><%=Html.PublishedState(Model.Item, T) %> | <%Html.Zone("meta");%></div>
<div class="postsummary">
<% Html.Zone("primary"); %>
</div>