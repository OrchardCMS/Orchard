<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CreateBlogPostViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<h2><%=Html.TitleForPage("Add Post") %></h2>
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <%=Html.ValidationSummary() %>
    <%=Html.EditorForItem(m => m.BlogPost) %><%
   } %>