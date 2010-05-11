<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<TenantsAddViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.MultiTenancy.ViewModels"%>
<% Html.RegisterFootScript("multitenancy.js"); %>
<h1><%=Html.TitleForPage(T("Add New Tenant").ToString()) %></h1> 
<%using (Html.BeginFormAntiForgeryPost()) { %>
    <%= Html.ValidationSummary() %>
    <fieldset>
        <div>
            <label for="Name"><%=_Encoded("Name") %></label>
            <input id="Name" class="textMedium" name="Name" type="text" />
        </div>
        <div>
            <label for="RequestUrlHost"><%=_Encoded("Host") %></label>
            <input id="RequestUrlHost" class="textMedium" name="RequestUrlHost" type="text" />
            <span class="hint"><%=_Encoded("Example: If host is \"orchardproject.net\", the tenant site URL is \"http://orchardproject.net/\"") %></span>
        </div>
    </fieldset>
    <fieldset>
        <legend><%=_Encoded("Database Setup") %></legend>
 	    <div>
	        <input type="radio" name="<%=Html.NameOf(m => m.DatabaseOptions) %>" value="" id="tenantDatabaseOption" <%=Model.DatabaseOptions == null ? " checked=\"checked\"" : "" %>/>
	        <label for="tenantDatabaseOption" class="forcheckbox"><%=_Encoded("Allow the client to set up the database") %></label>
	    </div>
        <div>
            <%=Html.RadioButtonFor(svm => svm.DatabaseOptions, true, new { id = "builtinDatabaseOption" })%>
            <label for="builtinDatabaseOption" class="forcheckbox"><%=_Encoded("Use built-in data storage (SQLite)") %></label>
        </div>
        <div>
            <%=Html.RadioButtonFor(svm => svm.DatabaseOptions, false, new { id = "sqlDatabaseOption" })%>
            <label for="sqlDatabaseOption" class="forcheckbox"><%=_Encoded("Use an existing SQL Server (or SQL Express) database") %></label>
            <span data-controllerid="sqlDatabaseOption">
            <label for="DatabaseConnectionString"><%=_Encoded("Connection string") %></label>
            <%=Html.EditorFor(svm => svm.DatabaseConnectionString)%>
            <span class="hint"><%=_Encoded("Example:") %><br /><%=_Encoded("Data Source=sqlServerName;Initial Catalog=dbName;Persist Security Info=True;User ID=userName;Password=password") %></span>
            </span>
            <span data-controllerid="sqlDatabaseOption">
            <label for="DatabaseTablePrefix"><%=_Encoded("Database Table Prefix") %></label>
            <%=Html.EditorFor(svm => svm.DatabaseTablePrefix)%>
            </span>
        </div>
    </fieldset>
	<fieldset>
	    <input type="submit" class="button primaryAction" value="<%=_Encoded("Save") %>" />
    </fieldset>
 <% } %>