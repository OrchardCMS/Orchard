<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<BlogPart>>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<%@ Import Namespace="Orchard.Core.Common.Extensions" %>
<h3><%: Html.Link(Model.Item.Title, Url.BlogPost(Model.Item)) %></h3>
<div class="meta"><%: Display.PublishedState(versionPublishedUtc: Model.Item)%> | <%Html.Zone("meta");%></div>
<div class="postsummary">
<% Html.Zone("primary"); %>
</div>