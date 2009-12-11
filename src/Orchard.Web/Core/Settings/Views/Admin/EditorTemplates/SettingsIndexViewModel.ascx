<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<SettingsIndexViewModel>" %>
<%@ Import Namespace="Orchard.Core.Settings.ViewModels"%>
<%@ Import Namespace="Orchard.Utility" %>
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
<% foreach(var e in Model.EditorModel.Editors) { %>
     <%=Html.EditorFor(m => e.Model, e.TemplateName, e.Prefix)%>
<% } %>
