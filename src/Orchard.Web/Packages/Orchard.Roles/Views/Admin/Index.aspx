<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.Roles.ViewModels.RolesIndexViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<% Html.Include("AdminHead"); %>
    <% Html.BeginForm(); %>
    <div class="yui-g">
        <h2 class="separator">Manage Roles</h2>
            <ol class="horizontal actions floatLeft">
		        <li><label class="floatLeft" for="bulkActions">Actions:</label>
				    <select id="Select1" name="roleActions">
					    <option value="1">Delete</option>
					</select> </li>
				<li>
				<input class="button roundCorners" type="submit" value="Apply" />
				</li>
			</ol>
        
        <%=Html.ValidationSummary() %>
        <%=Html.ActionLink("Add a new role", "Create", new {}, new {@class="floatRight topSpacer"}) %>
        <table id="Table1" cellspacing="0" class="roundCorners clearLayout" summary="This is a table of the roles currently available for use in your application.">
            <colgroup>
                <col id="Col1" />
			    <col id="Col2" />
				<col id="Col3" />
			</colgroup>
			<thead>
			    <tr>
				    <th scope="col"><%--<input type="checkbox" value="" />--%></th>
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
    </div>
    <% Html.EndForm(); %>
<% Html.Include("AdminFoot"); %>