<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.CmsPages.ViewModels.PageCreateViewModel>" %>
<h2><%=Html.TitleForPage("Add a Page") %></h2>
<p>Select your layout from one of the templates below.</p>
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <%=Html.ValidationSummary() %>
    <%=Html.EditorForModel() %>
<% } %>