<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<PageCreateViewModel>" %>
<%@ Import Namespace="Orchard.CmsPages.Services.Templates" %>
<%@ Import Namespace="Orchard.CmsPages.ViewModels" %>
<%@ Import Namespace="Orchard.Utility" %>
<fieldset>
    <ol class="templates">
        <% foreach (var template in Model.Templates) {
              var t = template; %>
            <li>
                <%=Html.EditorFor(m => t) %>
            </li>
        <% } %>
    </ol>
</fieldset>
<div><input class="button" type="submit" value="Create" /></div>