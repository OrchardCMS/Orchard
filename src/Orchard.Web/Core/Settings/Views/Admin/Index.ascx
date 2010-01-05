<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Orchard.Core.Settings.ViewModels.SettingsIndexViewModel>" %>

<h2>
    <%=Html.TitleForPage("Edit Settings")%></h2>
<%using (Html.BeginFormAntiForgeryPost()) { %>
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

<%= Html.EditorForItem(Model.ViewModel) %>

<fieldset>
    <input class="button" type="submit" value="Save" />
</fieldset>
<% } %>