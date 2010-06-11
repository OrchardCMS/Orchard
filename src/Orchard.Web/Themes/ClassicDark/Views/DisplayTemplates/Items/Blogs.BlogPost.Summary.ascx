<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<BlogPost>>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<%Model.Zones.AddRenderPartial("zonetest", "ZoneTest", Model); %>
<h2><%: Html.Link(Model.Item.Title, Url.BlogPost(Model.Item)) %></h2>
<div class="meta"><%=Html.PublishedState(Model.Item, T) %> | <%Html.Zone("meta");%></div>
<div class="postsummary">
<% Html.Zone("primary"); %>
</div>