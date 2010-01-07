<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<AdminBlogsViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<h1><%=Html.TitleForPage(T("Manage Blogs").ToString()) %></h1>
<p><%=_Encoded("Possible text about setting up and managing a blog goes here.") %></p><%
if (Model.Blogs.Count() > 0) { %>
<div class="actions"><a class="add button" href="<%=Url.BlogCreate() %>"><%=_Encoded("New Blog") %></a></div>
<%=Html.UnorderedList(Model.Blogs, (b, i) => Html.DisplayForItem(b).ToHtmlString(), "blogs contentItems") %>
<div class="actions"><a class="add button" href="<%=Url.BlogCreate() %>"><%=_Encoded("New Blog") %></a></div><%
} else { %>
<%-- todo: (heskew) come back to this --%>
<div class="info message"><%--<%=string.Format(_Encoded("There are no blogs for you to see. Want to {0}?").ToString(), Html.Link(_Encoded("add one").ToString(), Url.BlogCreate())) %>--%>
<%=string.Format("There are no blogs for you to see. Want to {0}?", Html.Link(_Encoded("add one").ToString(), Url.BlogCreate())) %></div><%
} %>