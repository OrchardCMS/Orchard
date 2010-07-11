<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<TagsAdminCreateViewModel>" %>
<%@ Import Namespace="Orchard.Tags.ViewModels"%>
<h1><%: Html.TitleForPage(T("Add a Tag").ToString()) %></h1>
<% using(Html.BeginFormAntiForgeryPost()) { %>
    <%: Html.ValidationSummary() %>
    <fieldset>
        <label for="TagName"><%: T("Name:")%></label>
	    <input id="TagName" class="text" name="TagName" type="text" value="<%: Model.TagName %>" />
        <input type="submit" class="button" value="<%: T("Save") %>" />
    </fieldset>
<% } %>