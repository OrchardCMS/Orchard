<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.Core.Settings.ViewModels.SettingsIndexViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<h2>Edit Settings</h2>
<%using (Html.BeginForm()) { %>
    <%= Html.ValidationSummary() %>
    <%= Html.EditorForModel() %>
    <fieldset>
        <input class="button" type="submit" value="Save" /> 
    </fieldset>
<% } %>