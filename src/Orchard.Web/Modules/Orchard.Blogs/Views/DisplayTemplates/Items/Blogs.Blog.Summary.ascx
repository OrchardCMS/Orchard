<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<BlogPart>>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<h2><%: Html.Link(Model.Item.Name, Url.Blog(Model.Item.Slug)) %></h2>
<% if (!string.IsNullOrEmpty(Model.Item.Description)) { %><p><%: Model.Item.Description %></p><% } %>
<div class="blog metadata"><%: T.Plural("1 post", "{0} posts", Model.Item.PostCount)%> | <%Html.Zone("meta");%></div>
