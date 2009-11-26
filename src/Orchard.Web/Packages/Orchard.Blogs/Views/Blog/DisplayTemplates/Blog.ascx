<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Blog>" %>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<h3><a href="<%=Url.BlogEdit(Model.Slug) %>"><%=Html.Encode(Model.Name) %></a> <span>(<a href="<%=Url.Blog(Model.Slug) %>">view</a>)</span> <span>(<a href="<%=Url.BlogPostCreate(Model.Slug) %>">post</a>)</span></h3>
<p>[list of authors] [modify blog access]</p>
<p><%=Model.Description %></p>
