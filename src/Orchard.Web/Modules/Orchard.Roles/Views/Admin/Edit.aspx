<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<RoleEditViewModel>" %>
<%@ Import Namespace="Orchard.Roles.ViewModels"%>
<h1><%=Html.TitleForPage(T("Edit Role").ToString()) %></h1>
<% using(Html.BeginFormAntiForgeryPost()) { %>
    <%=Html.ValidationSummary() %>
    <fieldset>
	    <legend><%=_Encoded("Information") %></legend>
	    <label for="pageTitle"><%=_Encoded("Role Name:") %></label>
	    <input id="Name" class="text" name="Name" type="text" value="<%=Html.Encode(Model.Name) %>" />
	    <input type="hidden" value="<%=Model.Id %>" name="Id" />
	</fieldset>
    <fieldset>
        <legend><%=_Encoded("Permissions") %></legend>
		<% foreach (var moduleName in Model.ModulePermissions.Keys) { %>
        <fieldset>
            <legend><%=_Encoded("{0} Module", moduleName) %></legend>
		    <table class="items">
			    <colgroup>
				    <col id="Col1" />
				    <col id="Col2" />
			    </colgroup>
			    <thead>
				    <tr>
					    <th scope="col"><%=_Encoded("Permission") %></th>
					    <th scope="col"><%=_Encoded("Allow") %></th>
					    <th scope="col"><%=_Encoded("Effective") %></th>
				    </tr>
			    </thead>
			    <% foreach (var permission in Model.ModulePermissions[moduleName]) { %>
                <tr>
				    <td><%=Html.Encode(permission.Description) %></td>
				    <td style="width:60px;/* todo: (heskew) make not inline :(">
				        <% if (Model.CurrentPermissions.Contains(permission.Name)) { %>
				            <input type="checkbox" value="true" name="<%=_Encoded("Checkbox.{0}", permission.Name) %>" checked="checked"/>
				        <% } else {%>
				            <input type="checkbox" value="true" name="<%=_Encoded("Checkbox.{0}", permission.Name) %>"/>
				        <% }%>
				    </td>	
				    <td style="width:60px;/* todo: (heskew) make not inline :(">
				    <% if (Model.EffectivePermissions.Contains(permission.Name)) { %>
				            <input type="checkbox" disabled="disabled" name="<%=_Encoded("Effective.{0}", permission.Name) %>" checked="checked"/>
				    <% } else {%>
				            <input type="checkbox" disabled="disabled" name="<%=_Encoded("Effective.{0}", permission.Name) %>"/>
				        <% }%>
				    </td>			    
			    </tr>
			    <% } %>
			</table>
			</fieldset>
			<% } %>
    </fieldset>
    <fieldset>
	    <input type="submit" class="button" name="submit.Save" value="<%=_Encoded("Save") %>" />
	    <input type="submit" class="button remove" name="submit.Delete" value="<%=_Encoded("Remove") %>" />
	</fieldset>
<% } %>