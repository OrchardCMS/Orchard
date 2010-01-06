<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<object>" %>
<h1><%=Html.TitleForPage(T("Install Theme").ToString()) %></h1>
<% using (Html.BeginForm("Install", "Admin", FormMethod.Post, new { enctype = "multipart/form-data" })) {%>
    <%=Html.ValidationSummary() %>
    <fieldset>
        <label for="ThemeZipPath"><%=_Encoded("File Path to the zip file:")%></label>
        <input id="ThemeZipPath" name="ThemeZipPath" type="file" class="text" value="<%=_Encoded("Browse") %>" size="64" /><br />
		<input type="submit" class="button" value="<%=_Encoded("Install") %>" />
		<%=Html.AntiForgeryTokenOrchard() %>
	</fieldset>
<% } %> 