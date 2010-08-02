<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<BlogEditViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<h1><%: Html.TitleForPage(T("Blog Properties").ToString()) %></h1>
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <%: Html.ValidationSummary() %>
    <%: Html.EditorForItem(m => m.Blog) %><%
   } %>