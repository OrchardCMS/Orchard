<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<NavigationManagementViewModel>" %>
<%@ Import Namespace="Orchard.ContentManagement"%>
<%@ Import Namespace="Orchard.Core.Navigation.Models"%>
<%@ Import Namespace="Orchard.Core.Navigation.ViewModels"%>
<h1><%=Html.TitleForPage(T("Manage Main Menu").ToString())%></h1><%
using (Html.BeginFormAntiForgeryPost()) { %>
<table class="items">
    <colgroup>
        <col id="Text" />
        <col id="Position" />
        <col id="Url" />
        <col id="Actions" />
    </colgroup>
    <thead>
        <tr>
            <td scope="col"><%=_Encoded("Text") %></td>
            <td scope="col"><%=_Encoded("Position") %></td>
            <td scope="col"><%=_Encoded("Url") %></td>
            <td scope="col"></td>
        </tr>
    </thead>
    <tbody><%
    var menuPartEntryIndex = 0;
    foreach (var menuPartEntry in Model.MenuItemEntries) {
        var i = menuPartEntryIndex; %>
        <tr>
            <td><input type="text" class="text-box" name="<%=Html.NameOf(m => m.MenuItemEntries[i].MenuItem.Text) %>" value="<%=menuPartEntry.MenuItem.Text %>" /></td>
            <td><input type="text" class="text-box" name="<%=Html.NameOf(m => m.MenuItemEntries[i].MenuItem.Position) %>" value="<%=menuPartEntry.MenuItem.Position %>" /></td>
            <td><% if (!menuPartEntry.IsMenuItem) { %><input type="text" class="text-box disabled" disabled="disabled" value="<%=menuPartEntry.MenuItem.Url %>" /><% } else { %><input type="text" class="text-box" name="<%=Html.NameOf(m => m.MenuItemEntries[i].MenuItem.Url) %>" value="<%=menuPartEntry.MenuItem.Url %>" /><% } %></td>
            <td><input type="hidden" name="<%=Html.NameOf(m => m.MenuItemEntries[i].MenuItemId) %>" value="<%=menuPartEntry.MenuItemId %>" /><a href="<%=Html.AntiForgeryTokenGetUrl(Url.Action("Delete", new {id = menuPartEntry.MenuItemId})) %>" class="remove"><%=_Encoded("Remove") %></a></td>
        </tr><%
        ++menuPartEntryIndex;
    } %>
    </tbody>
</table>
<fieldset class="actions"><button type="submit" class="button primaryAction"><%=_Encoded("Update All") %></button></fieldset><%     
}
%>

<h2><%=_Encoded("Add New Item") %></h2><%
using (Html.BeginFormAntiForgeryPost(Url.Action("create"), FormMethod.Post)) { %>
<table class="menu items">
    <colgroup>
        <col id="AddText" />
        <col id="AddPosition" />
        <col id="AddUrl" />
        <col id="AddActions" />
    </colgroup>
    <tbody>
        <tr>
            <td>
                <label for="MenuText"><%=_Encoded("Text") %></label>
                <%=Html.EditorFor(nmvm => nmvm.NewMenuItem.MenuItem.Item.As<MenuPart>().MenuText) %>
            </td>
            <td>
                <label for="MenuPosition"><%=_Encoded("Position")%></label>
                <%=Html.EditorFor(nmvm => nmvm.NewMenuItem.MenuItem.Item.As<MenuPart>().MenuPosition) %>
            </td>
            <td>
                <label for="Url"><%=_Encoded("Url")%></label>
                <%=Html.EditorFor(nmvm => nmvm.NewMenuItem.MenuItem.Item.As<Orchard.Core.Navigation.Models.MenuItem>().Url)%>
            </td>
            <td><button class="add" type="submit"><%=_Encoded("Add") %></button></td>
        </tr>
    </tbody>
</table><%
} %>