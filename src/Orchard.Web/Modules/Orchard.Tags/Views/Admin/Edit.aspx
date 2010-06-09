<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<TagsAdminEditViewModel>" %>
<%@ Import Namespace="Orchard.Tags.ViewModels"%>
<h1><%: Html.TitleForPage(T("Edit a Tag").ToString()) %></h1>
<% using(Html.BeginFormAntiForgeryPost()) { %>
    <%: Html.ValidationSummary() %>
    <fieldset>
        <label for="Name"><%: T("Name:") %></label>
        <input id="Id" name="Id" type="hidden" value="<%=Model.Id %>" />
        <input id="TagName" class="text" name="TagName" type="text" value="<%: Model.TagName %>" />
	    <input type="submit" class="button" value="<%: T("Save") %>" />
	</fieldset>
<% } %>