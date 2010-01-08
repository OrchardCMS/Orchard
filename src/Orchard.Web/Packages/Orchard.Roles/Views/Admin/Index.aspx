<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<RolesIndexViewModel>" %>
<%@ Import Namespace="Orchard.Roles.ViewModels"%>
<h1><%=Html.TitleForPage(T("Manage Roles").ToString())%></h1>
<% using(Html.BeginFormAntiForgeryPost()) { %>
    <%=Html.ValidationSummary() %>
    <fieldset class="actions bulk">
        <label for="publishActions"><%=_Encoded("Actions:") %></label>
        <select id="Select1" name="roleActions">
            <option value="1"><%=_Encoded("Delete") %></option>
        </select>
		<input class="button" type="submit" value="<%=_Encoded("Apply") %>" />
	</fieldset>
    <div class="manage"><%=Html.ActionLink(T("Add a role").ToString(), "Create", new {}, new { @class = "button" }) %></div>
    <fieldset>
        <table class="items" summary="<%=_Encoded("This is a table of the roles currently available for use in your application.") %>">
            <colgroup>
                <col id="Col1" />
		        <col id="Col2" />
			    <col id="Col3" />
		    </colgroup>
		    <thead>
		        <tr>
			        <th scope="col">&nbsp;&darr;<%-- todo: (heskew) something more appropriate for "this applies to the bulk actions --%></th>
				    <th scope="col"><%=_Encoded("Name")%></th>
				    <th scope="col"></th>
			    </tr>
		    </thead>
		    <%foreach (var row in Model.Rows) { %>
                <tr>
                    <td><input type="checkbox" value="true" name="<%="Checkbox." + row.Id %>"/></td>
                    <td><%=Html.Encode(row.Name) %></td>
                    <td><%=Html.ActionLink(T("Edit").ToString(), "Edit", new { row.Id })%></td>
                </tr>
            <%}%>
        </table>
    </fieldset>
<% } %>