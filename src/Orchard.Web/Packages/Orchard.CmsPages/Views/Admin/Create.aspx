<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.CmsPages.ViewModels.PageCreateViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<h2>Add a Page</h2>
<p>Select your layout from one of the templates below.</p>
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <%=Html.ValidationSummary() %>
    <%=Html.EditorForModel() %>
<% } %>