<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<SettingsIndexViewModel>" %>
<%@ Import Namespace="Orchard.Core.Settings.ViewModels"%>
<h1><%=Html.TitleForPage(T("Edit Settings").ToString())%></h1>
<%using (Html.BeginFormAntiForgeryPost()) { %>
<%= Html.ValidationSummary() %>
<fieldset>
    <legend><%=_Encoded("Global Settings")%></legend>
    <div>
        <%=Html.LabelFor(x=>x.SiteName) %>
        <%=Html.EditorFor(x=>x.SiteName) %>
        <%=Html.ValidationMessage("SiteName", "*") %>
    </div>
    <div>
        <%=Html.LabelFor(x => x.PageTitleSeparator) %>
        <%=Html.EditorFor(x => x.PageTitleSeparator)%>
        <%=Html.ValidationMessage("PageTitleSeparator", "*")%>
    </div>
    <div>
        <%=Html.LabelFor(x => x.SuperUser) %>
        <%=Html.EditorFor(x=>x.SuperUser) %>
        <%=Html.ValidationMessage("SuperUser", "*") %>
    </div>
</fieldset>
<%= Html.EditorForItem(Model.ViewModel) %>
<fieldset>
    <%=Html.EditorFor(s => s.Id) %>
    <input class="button" type="submit" value="<%=_Encoded("Save") %>" />
</fieldset>
<% } %>