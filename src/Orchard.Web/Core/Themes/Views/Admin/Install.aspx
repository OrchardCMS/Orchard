<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<% Html.Include("AdminHead"); %>
	<h2>Install Theme</h2>
    <% using (Html.BeginForm("Install", "Admin", FormMethod.Post, new { enctype = "multipart/form-data" })) {%>
        <%= Html.ValidationSummary() %>
        <fieldset>
            <label for="pageTitle">File Path to the zip file:</label>
            <input id="ThemeZipPath" name="ThemeZipPath" type="file" class="text" value="Browse" size="64"/><br />
			<input type="submit" class="button" value="Install" />
		</fieldset>
    <% } %> 
<% Html.Include("AdminFoot"); %>