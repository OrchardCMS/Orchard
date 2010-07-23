<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Packaging.ViewModels.PackagingAddSourceViewModel>" %>
<h1>
    <%: Html.TitleForPage(T("Add a Feed").ToString())%></h1>

<%using ( Html.BeginFormAntiForgeryPost(Url.Action("AddSource")) ) { %>
    <%: Html.ValidationSummary() %>
    <fieldset>
        <label for="Title"><%: T("Feed Title") %></label>
		<input id="Title" class="textMedium" name="Title" type="text" value="<%: Model.Title %>" />
        <label for="Url"><%: T("Feed Url") %></label>
		<input id="Url" class="textMedium" name="Url" type="text" value="<%: Model.Url %>"/>
    </fieldset>
	<fieldset>
	    <input type="submit" class="button primaryAction" value="<%: T("Add Feed") %>" />
    </fieldset>
 <% } %>