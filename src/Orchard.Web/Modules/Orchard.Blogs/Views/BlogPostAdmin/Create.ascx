<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<CreateBlogPostViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<h1><%: Html.TitleForPage(T("Create New Blog Post").ToString()) %></h1>
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <%: Html.ValidationSummary() %>
    <%: Html.EditorForItem(m => m.BlogPost) %><%
   } %>