<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<NavigationIndexViewModel>" %>
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
    <tbody>
        <%-- loop over menu items --%>
        <tr>
            <td><input type="text" name="text" /></td>
            <td><input type="text" name="position" /></td>
            <td><input type="text" name="url" /></td>
            <td>Delete Button</td>
        </tr>
        <%-- end loop --%>
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