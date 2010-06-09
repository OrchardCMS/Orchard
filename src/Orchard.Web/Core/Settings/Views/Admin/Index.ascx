<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<SettingsIndexViewModel>" %>
<%@ Import Namespace="Orchard.Core.Settings.ViewModels"%>
<h1><%: Html.TitleForPage(T("Manage Settings").ToString())%></h1>
<%using (Html.BeginFormAntiForgeryPost()) { %>
<%: Html.ValidationSummary() %>
<fieldset>
    <legend><%: T("Global Settings")%></legend>
    <div>
        <label for="SiteName"><%: T("Site name") %></label>
        <%: Html.EditorFor(m => m.SiteName)%>
        <%: Html.ValidationMessage("SiteName", "*") %>
    </div>
    <div>
        <label for="PageTitleSeparator"><%: T("Page title separator") %></label>
        <%: Html.EditorFor(x => x.PageTitleSeparator)%>
        <%: Html.ValidationMessage("PageTitleSeparator", "*")%>
    </div>
    <div>
        <label for="SuperUser"><%: T("Super user") %></label>
        <%: Html.EditorFor(x=>x.SuperUser) %>
        <%: Html.ValidationMessage("SuperUser", "*") %>
    </div>
</fieldset>
<%: Html.EditorForItem(Model.ViewModel) %>
<fieldset>
    <%: Html.EditorFor(s => s.Id) %>
    <input class="button primaryAction" type="submit" value="<%: T("Save") %>" />
</fieldset>
<% } %>
