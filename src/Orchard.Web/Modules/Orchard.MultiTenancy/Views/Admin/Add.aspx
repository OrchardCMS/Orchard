<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<TenantsAddViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.MultiTenancy.ViewModels"%>

<h1><%=Html.TitleForPage(T("Add a Tenant to your Site").ToString()) %></h1> 
			
<%using (Html.BeginFormAntiForgeryPost()) { %>
    <%= Html.ValidationSummary() %>
    <fieldset>
        <label for="Name"><%=_Encoded("Tenant Name") %></label>
		<input id="Name" class="textMedium" name="Name" type="text" /><br />
		<label for="DataProvider"><%=_Encoded("Data Provider Name") %></label>
	    <input id="DataProvider" class="textMedium" name="DataProvider" type="text" /><br />
	    <label for="Name"><%=_Encoded("Connection String") %></label>
	    <input id="ConnectionString" class="textMedium" name="ConnectionString" type="text" /><br />
	    <label for="Name"><%=_Encoded("Prefix") %></label>
	    <input id="Prefix" class="textMedium" name="Prefix" type="text" /><br />
    </fieldset>
	<fieldset>
	    <input type="submit" class="button primaryAction" value="<%=_Encoded("Save") %>" />
    </fieldset>
 <% } %>