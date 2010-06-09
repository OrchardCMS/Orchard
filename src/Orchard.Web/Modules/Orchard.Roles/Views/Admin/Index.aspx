<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<RolesIndexViewModel>" %>
<%@ Import Namespace="Orchard.Roles.ViewModels"%>
<h1><%: Html.TitleForPage(T("Manage Roles").ToString())%></h1>
<% using(Html.BeginFormAntiForgeryPost()) { %>
    <%: Html.ValidationSummary() %>
    <fieldset class="actions bulk">
        <label for="publishActions"><%: T("Actions:") %></label>
        <select id="Select1" name="roleActions">
            <option value="1"><%: T("Remove") %></option>
        </select>
		<input class="button" type="submit" value="<%: T("Apply") %>" />
	</fieldset>
    <div class="manage"><%: Html.ActionLink(T("Add a role").ToString(), "Create", new { }, new { @class = "button primaryAction" })%></div>
    <fieldset>
        <table class="items" summary="<%: T("This is a table of the roles currently available for use in your application.") %>">
            <colgroup>
                <col id="Col1" />
		        <col id="Col2" />
			    <col id="Col3" />
		    </colgroup>
		    <thead>
		        <tr>
			        <th scope="col">&nbsp;&darr;<%-- todo: (heskew) something more appropriate for "this applies to the bulk actions --%></th>
				    <th scope="col"><%: T("Name")%></th>
				    <th scope="col"></th>
			    </tr>
		    </thead>
		    <%foreach (var row in Model.Rows) { %>
                <tr>
                    <td><input type="checkbox" value="true" name="<%="Checkbox." + row.Id %>"/></td>
                    <td><%: row.Name %></td>
                    <td><%: Html.ActionLink(T("Edit").ToString(), "Edit", new { row.Id })%></td>
                </tr>
            <%}%>
        </table>
    </fieldset>
<% } %>