<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BlogForAdminViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<h2><a href="<%=Url.BlogForAdmin(Model.Blog.Slug) %>"><%=Html.Encode(Model.Blog.Name) %></a></h2>
<p><%=Model.Blog.Description %></p>
<div><a href="<%=Url.BlogEdit(Model.Blog.Slug) %>">(edit)</a></div><%
//TODO: (erikpo) Move this into a helper
if (Model.Posts.Count() > 0) { %>
<ul><%
    foreach (BlogPost post in Model.Posts) { %>
    <li><% Html.RenderPartial("BlogPostPreviewForAdmin", post); %></li><%
    } %>
</ul><%
} %>
