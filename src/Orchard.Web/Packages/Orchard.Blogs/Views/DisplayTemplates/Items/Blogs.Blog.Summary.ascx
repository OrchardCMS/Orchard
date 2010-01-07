<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<Blog>>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<h2><%=Html.Link(Html.Encode(Model.Item.Name), Url.Blog(Model.Item.Slug)) %></h2>
<div class="blog metadata"><a href="<%=Url.Blog(Model.Item.Slug) %>"><%=_Encoded("{0} post{0}", Model.Item.PostCount, Model.Item.PostCount == 1 ? "" : "s")%></a></div>
<p><%=Html.Encode(Model.Item.Description) %></p>