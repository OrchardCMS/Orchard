<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<NavigationManagementViewModel>" %>
<%@ Import Namespace="Orchard.ContentManagement"%>
<%@ Import Namespace="Orchard.Core.Navigation.Models"%>
<%@ Import Namespace="Orchard.Core.Navigation.ViewModels"%><%
var menu = Model.Menu.FirstOrDefault(); %>
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
    foreach (var menuItem in menu.Items) { %>
        <tr>
            <td><%=Html.TextBox("text", menuItem.Text) %></td>
            <td><%=Html.TextBox("position", menuItem.Position) %></td>
            <td><%=Html.TextBox("url", menuItem.Url) %></td>
            <td><a href="#" class="remove button">delete</a></td>
        </tr><%
    } %>
    </tbody>
</table>
<fieldset class="actions"><button type="submit"><%=_Encoded("Update All") %></button></fieldset><%     
}
%>

<h2><%=_Encoded("Add New Item") %></h2><%
using (Html.BeginFormAntiForgeryPost("/admin/navigation/create", FormMethod.Post)) { %>
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