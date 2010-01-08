<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<RoleCreateViewModel>" %>
<%@ Import Namespace="Orchard.Roles.ViewModels"%>
<h1><%=Html.TitleForPage(T("Add Role").ToString()) %></h1>
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <%=Html.ValidationSummary()%>
    <fieldset>
	    <legend><%=_Encoded("Information") %></legend>
	    <label for="pageTitle"><%=_Encoded("Role Name:") %></label>
	    <input id="Name" class="text" name="Name" type="text" value="<%=Html.Encode(Model.Name) %>" />
    </fieldset>
    <fieldset>
        <legend><%=_Encoded("Permissions") %></legend>
        <% foreach (var packageName in Model.PackagePermissions.Keys) { %>
        <fieldset>
            <legend><%=_Encoded("{0} Module", packageName) %></legend>
            <table class="items">
                <colgroup>
                    <col id="Permission" />
                    <col id="Allow" />
                </colgroup>
                <thead>
                    <tr>
                        <th scope="col"><%=_Encoded("Permission") %></th>
                        <th scope="col"><%=_Encoded("Allow") %></th>
                    </tr>
                </thead>
                <% foreach (var permission in Model.PackagePermissions[packageName]) { %>
                <tr>
                    <td><%=Html.Encode(permission.Description) %></td>
                    <td style="width:60px;/* todo: (heskew) make not inline :( */"><input type="checkbox" value="true" name="<%=_Encoded("Checkbox.{0}", permission.Name) %>"/></td>
                </tr>
                <% } %>
            </table>
        </fieldset>
        <% } %>
    </fieldset>
    <fieldset>
       <input type="submit" class="button" value="<%=_Encoded("Save") %>" />
    </fieldset>
<% } %>