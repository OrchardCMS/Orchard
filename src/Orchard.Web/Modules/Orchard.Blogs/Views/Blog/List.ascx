<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<BlogsViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<%if (Model.Blogs.Count() > 0) { %>
<%: Html.UnorderedList(Model.Blogs, (b, i) => Html.DisplayForItem(b), "blogs contentItems") %><%
}
else { %>
<p><%: T("No blogs found.") %></p><%
} %>