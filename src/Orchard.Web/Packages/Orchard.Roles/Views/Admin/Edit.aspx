<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<RoleEditViewModel>" %>
<%@ Import Namespace="Orchard.Roles.ViewModels"%>

<%@ Import Namespace="Orchard.Mvc.Html" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Edit a Role</title>
    <% Html.Include("Head"); %>
</head>
<body>
    <% Html.Include("Header"); %>
                    <div class="yui-g">
						<h2 class="separator">Edit a Role</h2>
						<h3>Information</h3>
						<label for="pageTitle">Role Name:</label>
						<input id="pageTitle" class="inputText inputTextLarge" name="pageTitle" type="text" />
					
						<h3>Permissions</h3>
						
						<h4>Pages Module</h4>
						<table id="pluginListTable" cellspacing="0" class="roundCorners clearLayout" >
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
							<tr>
								<td>View pages</td>
								<td><input type="checkbox" value="" /></td>
							</tr>
							<tr>
								<td>Create draft pages</td>
								<td><input type="checkbox" value="" /></td>
							</tr>
							<tr>
								<td>Edit any page</td>
								<td><input type="checkbox" value="" /></td>
							</tr>
							</table>
								<input type="button" class="button" value="Save" />
								<input type="button" class="button" value="Delete" />
					</div>
    <% Html.Include("Footer"); %>
</body>
</html>
