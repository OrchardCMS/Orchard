<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<object>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<h1><%: Html.TitleForPage(T("Install Theme").ToString()) %></h1>
<% using (Html.BeginForm("Install", "Admin", FormMethod.Post, new { enctype = "multipart/form-data" })) {%>
    <%: Html.ValidationSummary() %>
    <fieldset>
        <label for="ThemeZipPath"><%: T("File Path to the zip file:")%></label>
        <input id="ThemeZipPath" name="ThemeZipPath" type="file" class="text" value="<%: T("Browse") %>" size="64" /><br />
		<input type="submit" class="button" value="<%: T("Install") %>" />
		<%: Html.AntiForgeryTokenOrchard() %>
	</fieldset>
<% } %> 