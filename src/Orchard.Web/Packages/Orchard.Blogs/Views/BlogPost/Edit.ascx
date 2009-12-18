<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BlogPostEditViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<% Html.Title("Edit Post"); %>
<h2>Edit Post</h2>
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <%=Html.ValidationSummary() %>
    <%=Html.EditorForItem(m => m.BlogPost) %><%
   } %>