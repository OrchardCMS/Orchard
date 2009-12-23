<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BlogsViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<% Html.AddTitleParts("Blogs"); %>
<h1>Blogs</h1>
<p>All of the blogs.</p><%
if (Model.Blogs.Count() > 0) { %>
<%=Html.UnorderedList(Model.Blogs, (b, i) => Html.DisplayForItem(b).ToHtmlString(), "blogs contentItems") %><%
}
else { %>
<p>No blogs found.</p><%
} %>