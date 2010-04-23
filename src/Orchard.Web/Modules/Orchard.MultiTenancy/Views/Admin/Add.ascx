<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<TenantsAddViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.MultiTenancy.ViewModels"%>

<h1><%=Html.TitleForPage(T("Add a Tenant to your Site").ToString()) %></h1> 
			
<%using (Html.BeginFormAntiForgeryPost()) { %>
    <%= Html.ValidationSummary() %>
    <fieldset>
        <label for="Name"><%=_Encoded("Tenant Name") %></label>
		<input id="Name" class="textMedium" name="Name" type="text" /><br />
	    <label for="RequestUrlHost"><%=_Encoded("Host Prefix") %></label>
		<input id="RequestUrlHost" class="textMedium" name="RequestUrlHost" type="text" /><br />
		<label for="RequestUrlPrefix"><%=_Encoded("Url Prefix") %></label>
		<input id="RequestUrlPrefix" class="textMedium" name="RequestUrlPrefix" type="text" /><br />
    </fieldset>
	<fieldset>
	    <input type="submit" class="button primaryAction" value="<%=_Encoded("Save") %>" />
    </fieldset>
 <% } %>