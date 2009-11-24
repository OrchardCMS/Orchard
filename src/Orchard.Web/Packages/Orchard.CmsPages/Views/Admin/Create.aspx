<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.CmsPages.ViewModels.PageCreateViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<% Html.Include("Head"); %>
    <h2>Add a Page</h2>
    <p>Select your layout from one of the templates below.</p>
    <%using (Html.BeginForm()) { %>
        <%=Html.ValidationSummary() %>
        <%=Html.EditorForModel() %>
    <% } %>
<% Html.Include("Foot"); %>