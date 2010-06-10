<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<Blog>>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<h3><%: Html.Link(Model.Item.Name, Url.Blog(Model.Item.Slug)) %></h3>
<div class="blog meta"><a href="<%=Url.Blog(Model.Item.Slug) %>"><%: T("{0} post{1}", Model.Item.PostCount, Model.Item.PostCount == 1 ? "" : "s")%></a></div>
<div class="blogdescription"><p><%: Model.Item.Description %></p></div>
