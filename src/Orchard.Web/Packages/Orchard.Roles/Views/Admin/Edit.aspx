<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<RoleEditViewModel>" %>
<%@ Import Namespace="Orchard.Roles.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<% Html.Include("AdminHead"); %>
    <% Html.BeginForm(); %>
    <%= Html.ValidationSummary() %>
                    <div class="yui-g">
						<h2 class="separator">Edit a Role</h2>
						<h3>Information</h3>
						<label for="pageTitle">Role Name:</label>
						<input id="Name" class="inputText inputTextLarge" name="Name" type="text" value="<%=Model.Name %>"/>
					    <input type="hidden" value="<%= Model.Id %>" name="Id" />
						<h3>Permissions</h3>
						
						<% foreach (var packageName in Model.PackagePermissions.Keys) { %>
						<h4><%= packageName %> Module</h4>
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
								<td>
								    <% if (Model.CurrentPermissions.Contains(permission.Name)) {%>
								        <input type="checkbox" value="true" name="<%="Checkbox." + permission.Name%>" checked="checked"/>
								    <% } else {%>
								        <input type="checkbox" value="true" name="<%="Checkbox." + permission.Name%>"/>
								    <% }%>
								</td>
							</tr>
							<% } %>
							</table>
							<% } %>
								<input type="submit" class="button" name="submit.Save" value="Save" />
								<input type="submit" class="button" name="submit.Delete" value="Delete" />
					</div>
	<% Html.EndForm(); %>
<% Html.Include("AdminFoot"); %>