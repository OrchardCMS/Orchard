<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BlogForAdminViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<h2><a href="<%=Url.BlogForAdmin(Model.Blog.Slug) %>"><%=Html.Encode(Model.Blog.Name) %></a></h2>
<div class="manage">
    <a href="<%=Url.BlogEdit(Model.Blog.Slug) %>" class="button">Edit Blog</a>
    <a href="<%=Url.BlogEdit(Model.Blog.Slug) %>" class="button">Add a Post</a>
</div>
<p><a href="<%=Url.BlogsForAdmin() %>">Manage Blogs</a> &gt; <%=Html.Encode(Model.Blog.Name) %></p>
<p><%=Model.Blog.Description %></p><%
//TODO: (erikpo) Move this into a helper
if (Model.Posts.Count() > 0) { %>
<ul class="posts"><%
    foreach (BlogPost post in Model.Posts) { %>
    <li><% Html.RenderPartial("BlogPostPreviewForAdmin", post); %></li><%
    } %>
</ul><%
} %>
