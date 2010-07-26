<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<BlogPostEditViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<h1><%: Html.TitleForPage(T("Edit Blog Post").ToString()) %></h1>
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <%: Html.ValidationSummary() %>
    <%: Html.EditorForItem(m => m.BlogPost) %><%
   } %>