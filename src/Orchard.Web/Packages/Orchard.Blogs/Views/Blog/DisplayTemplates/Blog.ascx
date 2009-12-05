<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Blog>" %>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<h3><a href="<%=Url.Blog(Model.Slug) %>"><%=Html.Encode(Model.Name) %></a></h3>
<div class="blog metadata"><a href="<%=Url.Blog(Model.Slug) %>"><%=Model.PostCount %> post<%=Model.PostCount == 1 ? "" : "s" %></a></div>
<p><%=Model.Description %></p>
