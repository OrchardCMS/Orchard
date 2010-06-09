<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<Blog>>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<h2><%=Html.Link(Html.Encode(Model.Item.Name), Url.Blog(Model.Item.Slug)) %></h2>
<% if (!string.IsNullOrEmpty(Model.Item.Description)) { %><p><%: Model.Item.Description %></p><% } %>
<div class="blog metadata"><%: T("{0} post{1}", Model.Item.PostCount, Model.Item.PostCount == 1 ? "" : "s")%> | <%Html.Zone("meta");%></div>
