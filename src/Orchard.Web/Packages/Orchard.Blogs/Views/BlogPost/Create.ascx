<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CreateBlogPostViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<h2>Add Post</h2>
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <%=Html.ValidationSummary() %>
    <%=Html.EditorForItem(m => m.BlogPost) %><%
   } %>