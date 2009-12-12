<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.Roles.ViewModels.RolesIndexViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<% Html.Include("AdminHead"); %>
    <h2>Manage Roles</h2>
    <% using(Html.BeginForm()) { %>
        <%=Html.ValidationSummary() %>
        <fieldset class="actions bulk">
            <label for="publishActions">Actions: </label>
            <select id="Select1" name="roleActions">
                <option value="1">Delete</option>
            </select>
			<input class="button" type="submit" value="Apply" />
		</fieldset>
        <div class="manage"><%=Html.ActionLink("Add a role", "Create", new {}, new { @class = "button" }) %></div>
        <fieldset>
            <table class="items" summary="This is a table of the roles currently available for use in your application.">
                <colgroup>
                    <col id="Col1" />
			        <col id="Col2" />
				    <col id="Col3" />
			    </colgroup>
			    <thead>
			        <tr>
				        <th scope="col">&nbsp;&darr;<%-- todo: (heskew) something more appropriate for "this applies to the bulk actions --%></th>
					    <th scope="col">Name</th>
					    <th scope="col"></th>
				    </tr>
			    </thead>
			    <%foreach (var row in Model.Rows) { %>
                    <tr>
                        <td><input type="checkbox" value="true" name="<%= "Checkbox." + row.Id %>"/></td>
                        <td><%=Html.Encode(row.Name) %></td>
                        <td><%=Html.ActionLink("Edit", "Edit", new { row.Id })%></td>
                    </tr>
                <%}%>
	        </table>
	    </fieldset>
    <% } %>
<% Html.Include("AdminFoot"); %>