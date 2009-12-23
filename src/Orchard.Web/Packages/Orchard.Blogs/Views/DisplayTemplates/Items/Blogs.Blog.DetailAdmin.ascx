<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ItemDisplayModel<Blog>>" %>
<%@ Import Namespace="Orchard.ContentManagement.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<%-- todo: (heskew) get what actions we can out of the h2 :| --%>
<h2 class="withActions">
    <a href="<%=Url.BlogForAdmin(Model.Item.Slug) %>"><%=Html.TitleForPage(Model.Item.Name) %></a>
    <a href="<%=Url.BlogEdit(Model.Item.Slug) %>" class="ibutton edit" title="Edit Blog">Edit Blog</a>
    <span class="actions"><span class="destruct"><a href="<%=Url.BlogDelete(Model.Item.Slug) %>" class="ibutton remove" title="Remove Blog">Remove Blog</a></span></span></h2>
<p><%=Model.Item.Description%></p>
<div class="actions"><a href="<%=Url.BlogPostCreate(Model.Item.Slug) %>" class="add button">New Post</a></div>
<%--TODO: (erikpo) Need to figure out which zones should be displayed in this template--%>
<%=Html.DisplayZonesAny() %>

<%--<%
if (Model.Posts.Count() > 0) { %>
<%=Html.UnorderedList(Model.Posts, (p, i) => Html.DisplayFor(blog => p, "BlogPostPreviewForAdmin").ToHtmlString(), "contentItems")%>
<div class="actions"><a href="<%=Url.BlogPostCreate(Model.Blog.Slug) %>" class="add button">New Post</a></div><%
} else { %>
<div class="info message">This blog is sad with no posts, but don't fret. You can add a new post right <a href="<%=Url.BlogPostCreate(Model.Blog.Slug) %>">here</a>!</div><%
} %>--%>