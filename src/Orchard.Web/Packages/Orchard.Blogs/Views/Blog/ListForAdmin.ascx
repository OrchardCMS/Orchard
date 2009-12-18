<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BlogsForAdminViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<% Html.Title("Manage Blogs"); %>
<h2>Manage Blogs</h2>
<p>Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.</p><%
if (Model.Blogs.Count() > 0) { %>
<div class="actions"><a class="add button" href="<%=Url.BlogCreate() %>">New Blog</a></div>
<%=Html.UnorderedList(Model.Blogs, (b, i) => Html.DisplayForItem(b).ToHtmlString(), "blogs contentItems") %>
<div class="actions"><a class="add button" href="<%=Url.BlogCreate() %>">New Blog</a></div><%
} else { %>
<div class="info message">There are no blogs for you to see. Want to <a href="<%=Url.BlogCreate() %>">add one</a>?</div><%
} %>