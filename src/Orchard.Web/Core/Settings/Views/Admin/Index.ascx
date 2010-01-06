<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<SettingsIndexViewModel>" %>
<%@ Import Namespace="Orchard.Core.Settings.ViewModels"%>
<h1><%=Html.TitleForPage(T("Edit Settings").ToString())%></h1>
<%using (Html.BeginFormAntiForgeryPost()) { %>
<%= Html.ValidationSummary() %>
<fieldset>
    <legend><%=_Encoded("Global Settings")%></legend>
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
    <input class="button" type="submit" value="<%=_Encoded("Save") %>" />
</fieldset>
<% } %>