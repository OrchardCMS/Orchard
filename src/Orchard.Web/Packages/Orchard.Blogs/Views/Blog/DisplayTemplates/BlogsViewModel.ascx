<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BlogsViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<%@ Import Namespace="Orchard.Blogs.ViewModels"
%><%
if (Model.Blogs.Count() > 0) { %>
<%=Html.UnorderedList(Model.Blogs, (b, i) => Html.DisplayFor(blog => b).ToHtmlString(), "blogs") %><%
}
else { %>
<p>No blogs found.</p><%
} %>