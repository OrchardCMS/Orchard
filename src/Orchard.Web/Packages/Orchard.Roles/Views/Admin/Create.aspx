<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<RoleCreateViewModel>" %>
<%@ Import Namespace="Orchard.Roles.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<h2>Add Role</h2>
<% using (Html.BeginForm()) { %>
    <%=Html.ValidationSummary()%>
    <fieldset>
	    <legend>Information</legend>
	    <label for="pageTitle">Role Name:</label>
	    <input id="Name" class="inputText inputTextLarge" name="Name" type="text" value="<%= Model.Name %>" />
    </fieldset>
    <fieldset>
        <legend>Permissions</legend>
        <% foreach (var packageName in Model.PackagePermissions.Keys) { %>
        <fieldset>
            <legend><%=packageName%> Module</legend>
            <table class="items">
                <colgroup>
                    <col id="Permission" />
                    <col id="Allow" />
                </colgroup>
                <thead>
                    <tr>
                        <th scope="col">Permission</th>
                        <th scope="col">Allow</th>
                    </tr>
                </thead>
                <% foreach (var permission in Model.PackagePermissions[packageName]) {%>
                <tr>
                    <td><%=permission.Description%></td>
                    <td style="width:60px;/* todo: (heskew) make not inline :("><input type="checkbox" value="true" name="<%="Checkbox." + permission.Name%>"/></td>
                </tr>
                <% } %>
            </table>
        </fieldset>
        <% } %>
    </fieldset>
    <fieldset>
       <input type="submit" class="button" value="Save" />
    </fieldset>
<% } %>