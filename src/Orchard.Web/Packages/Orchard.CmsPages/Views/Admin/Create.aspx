<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.CmsPages.ViewModels.PageCreateViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<% Html.Include("Head"); %>
    <div class="yui-u">
        <h2>Add a Page</h2>
        <p class="bottomSpacer">
            Select your layout from one of the templates below.</p>
    </div>
    <div class="yui-u">
        <%using (Html.BeginForm()) { %>
        <%= Html.ValidationSummary() %>
        <%= Html.EditorForModel() %>
        <%}/*EndForm*/%>
    </div>
<% Html.Include("Foot"); %>