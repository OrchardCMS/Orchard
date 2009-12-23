<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BlogPostEditViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<h2><%=Html.TitleForPage("Edit Post") %></h2>
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <%=Html.ValidationSummary() %>
    <%=Html.EditorForItem(m => m.BlogPost) %><%
   } %>