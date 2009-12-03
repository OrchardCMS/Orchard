<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BlogsForAdminViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<%@ Import Namespace="Orchard.Blogs.ViewModels"
%><div class="actions"><a class="add button" href="<%=Url.BlogCreate() %>">Add Blog</a></div><%
if (Model.Blogs.Count() > 0) { %>
<%=Html.UnorderedList(Model.Blogs, (b, i) => Html.DisplayFor(blog => b, "BlogForAdmin").ToHtmlString(), "contentItems") %>
<div class="actions"><a class="add button" href="<%=Url.BlogCreate() %>">Add Blog</a></div><%
} %>