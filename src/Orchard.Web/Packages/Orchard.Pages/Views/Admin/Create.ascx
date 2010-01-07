<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<PageCreateViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Pages.ViewModels"%>
<h2><%=Html.TitleForPage("Add Page") %></h2>
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <%=Html.ValidationSummary() %>
    <%=Html.EditorForItem(m => m.Page) %><%
   } %>