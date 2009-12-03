<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BlogsForAdminViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<%@ Import Namespace="Orchard.Blogs.ViewModels"
%><%
if (Model.Blogs.Count() > 0) { %>
<div class="actions"><a class="add button" href="<%=Url.BlogCreate() %>">New Blog</a></div>
<%=Html.UnorderedList(Model.Blogs, (b, i) => Html.DisplayFor(blog => b, "BlogForAdmin").ToHtmlString(), "contentItems") %>
<div class="actions"><a class="add button" href="<%=Url.BlogCreate() %>">New Blog</a></div><%
} else { %>
<div class="info message">There are no blogs for you to see. Want to <a href="<%=Url.BlogCreate() %>">add one</a>?</div><%
} %>