<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.Core.Settings.ViewModels.SettingsIndexViewModel>" %>

<%@ Import Namespace="Orchard.Mvc.Html" %>
<h2>
    Edit Settings</h2>
<%using (Html.BeginForm()) { %>
<%= Html.ValidationSummary() %>
<fieldset>
    <legend>Global Settings</legend>
    <fieldset>
        <%=Html.LabelFor(x=>x.SiteName) %>
        <%=Html.EditorFor(x=>x.SiteName) %>
        <%=Html.ValidationMessage("SiteName", "*") %>
    </fieldset>
    <fieldset>
        <%=Html.LabelFor(x => x.SuperUser) %>
        <%=Html.EditorFor(x=>x.SuperUser) %>
        <%=Html.ValidationMessage("SuperUser", "*") %>
    </fieldset>
    <%=Html.EditorFor(s=>s.Id) %>
</fieldset>
<% foreach (var e in Model.EditorModel.Editors) { %>
<%=Html.EditorFor(m => e.Model, e.TemplateName, e.Prefix)%>
<% } %>
<fieldset>
    <input class="button" type="submit" value="Save" />
</fieldset>
<% } %>