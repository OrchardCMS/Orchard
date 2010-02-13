<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<NavigationManagementViewModel>" %>
<%@ Import Namespace="Orchard.Core.Navigation.ViewModels"%><%
var menu = Model.Menu.FirstOrDefault(); %>
<h1><%=Html.TitleForPage(T("Edit Main Menu").ToString())%></h1><%
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

using (Html.BeginFormAntiForgeryPost()) { %>
<table class="items">
    <tbody>
        <tr>
            <td><input type="text" name="addtext" id="addtext" /></td>
            <td><input type="text" name="addposition" id="addposition" /></td>
            <td><input type="text" name="addurl" id="addurl" /></td>
            <td><button class="add" type="submit"><%=_Encoded("Add") %></button></td>
        </tr>
    </tbody>
</table><%
} %>