<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<Blog>>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>

<h3><%=Html.Link(Html.Encode(Model.Item.Name), Url.Blog(Model.Item.Slug)) %></h3>
<div class="blog meta"><a href="<%=Url.Blog(Model.Item.Slug) %>"><%: T.Plural("1 post", "{0} posts", Model.Item.PostCount)%></a></div>
<div class="blogdescription"><p><%: Model.Item.Description %></p></div>
