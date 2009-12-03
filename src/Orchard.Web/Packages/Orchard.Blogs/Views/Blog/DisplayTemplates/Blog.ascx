<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Blog>" %>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<h2>
    <a href="<%=Url.Blog(Model.Slug) %>"><%=Html.Encode(Model.Name) %></a>
    <span>(<%=Model.PostCount %> post<%=Model.PostCount == 1 ? "" : "s" %>)</span>
</h2>
<p><%=Model.Description %></p>
