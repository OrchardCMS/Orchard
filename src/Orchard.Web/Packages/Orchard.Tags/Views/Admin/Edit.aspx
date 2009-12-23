<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<TagsAdminEditViewModel>" %>
<%@ Import Namespace="Orchard.Tags.ViewModels"%>
<h2><%=Html.TitleForPage("Edit a Tag") %></h2>
<% using(Html.BeginFormAntiForgeryPost()) { %>
    <%=Html.ValidationSummary() %>
    <fieldset>
        <label for="Name">Name:</label>
        <input id="Id" name="Id" type="hidden" value="<%=Model.Id %>" />
        <input id="TagName" class="text" name="TagName" type="text" value="<%= Model.TagName %>" />
	    <input type="submit" class="button" value="Save" />
	</fieldset>
<% } %>