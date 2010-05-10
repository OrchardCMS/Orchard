<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<TenantsAddViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.MultiTenancy.ViewModels"%>
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
	        <input type="radio" name="" value="" />
	        <label for="" class="forcheckbox"><%=_Encoded("Allow the client to set up the database") %></label>
	    </div>
	    <div>
	        <input type="radio" name="" value="" />
	        <label for="" class="forcheckbox"><%=_Encoded("Use built-in data storage") %></label>
	    </div>
	    <div>
	        <input type="radio" name="" value="" />
	        <label for="" class="forcheckbox"><%=_Encoded("Use an existing SQL Server (or SQL Express) database") %></label>
	    </div>
   </fieldset>
	<fieldset>
	    <input type="submit" class="button primaryAction" value="<%=_Encoded("Save") %>" />
    </fieldset>
 <% } %>