<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.Core.Settings.ViewModels.SettingsIndexViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<h2>Edit Settings</h2>
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
        <%=Html.LabelFor(x => x.PageTitleSeparator) %>
        <%=Html.EditorFor(x => x.PageTitleSeparator)%>
        <%=Html.ValidationMessage("PageTitleSeparator", "*")%>
    </fieldset>
    <fieldset>
        <%=Html.LabelFor(x => x.SuperUser) %>
        <%=Html.EditorFor(x=>x.SuperUser) %>
        <%=Html.ValidationMessage("SuperUser", "*") %>
    </fieldset>
    <%=Html.EditorFor(s=>s.Id) %>
</fieldset>
<% foreach (var e in Model.EditorModel.Editors) {
    var editor = e;
    %><%=Html.EditorFor(m => editor.Model, editor.TemplateName, editor.Prefix)%>
<% } %>
<fieldset>
    <input class="button" type="submit" value="Save" />
</fieldset>
<% } %>