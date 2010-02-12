<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<NavigationManagementViewModel>" %>
<%@ Import Namespace="Orchard.Core.Navigation.ViewModels"%>
<h1><%=Html.TitleForPage(T("Edit Main Menu").ToString())%></h1><%
using (Html.BeginFormAntiForgeryPost()) { %>
<table>
    <thead>
        <tr>
            <td>Text</td>
            <td>Position</td>
            <td>Url</td>
            <td></td>
        </tr>
    </thead>
    <tbody><%
    foreach (var menuItem in Model.Menu) { %>
        <tr>
            <td><%=Html.TextBox("text", menuItem.Text) %></td>
            <td><%=Html.TextBox("position", menuItem.Position) %></td>
            <td><%=Html.TextBox("url", menuItem.Url) %></td>
            <td>Delete Button</td>
        </tr><%
    } %>
        <tr>
            <td></td>
            <td></td>
            <td></td>
            <td>Update All Button</td>
        </tr>
    </tbody>
</table><%     
}

using (Html.BeginFormAntiForgeryPost()) { %>
<table>
    <tbody>
        <tr>
            <td><input type="text" name="addtext" /></td>
            <td><input type="text" name="addposition" /></td>
            <td><input type="text" name="addurl" /></td>
            <td>Add Button</td>
        </tr>
    </tbody>
</table><%
} %>