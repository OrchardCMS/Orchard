<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<RoleEditViewModel>" %>
<%@ Import Namespace="Orchard.Roles.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<% Html.Include("AdminHead"); %>
	<h2>Edit Role</h2>
    <% using(Html.BeginForm()) { %>
        <%=Html.ValidationSummary() %>
        <fieldset>
		    <legend>Information</legend>
			<label for="pageTitle">Role Name:</label>
			<input id="Name" class="inputText inputTextLarge" name="Name" type="text" value="<%=Model.Name %>"/>
		    <input type="hidden" value="<%= Model.Id %>" name="Id" />
		</fieldset>
	    <fieldset>
	        <legend>Permissions</legend>
			<% foreach (var packageName in Model.PackagePermissions.Keys) { %>
            <fieldset>
                <legend><%=packageName%> Module</legend>
			    <table id="Table1" cellspacing="0" class="roundCorners clearLayout" >
				    <colgroup>
					    <col id="Col1" />
					    <col id="Col2" />
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
					    <td style="width:60px;/* todo: (heskew) make not inline :(">
					        <% if (Model.CurrentPermissions.Contains(permission.Name)) {%>
					            <input type="checkbox" value="true" name="<%="Checkbox." + permission.Name%>" checked="checked"/>
					        <% } else {%>
					            <input type="checkbox" value="true" name="<%="Checkbox." + permission.Name%>"/>
					        <% }%>
					    </td>
				    </tr>
				    <% } %>
				</table>
				</fieldset>
				<% } %>
	    </fieldset>
	    <fieldset>
		    <input type="submit" class="button" name="submit.Save" value="Save" />
		    <input type="submit" class="button" name="submit.Delete" value="Delete" />
		</fieldset>
	<% } %>
<% Html.Include("AdminFoot"); %>