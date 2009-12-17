<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<TagsAdminEditViewModel>" %>
<%@ Import Namespace="Orchard.Tags.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<h2>Edit a Tag</h2>
<% Html.BeginForm(); %>
    <%= Html.ValidationSummary() %>
    <fieldset>
        <label for="Name">Name:</label>
        <input id="Id" name="Id" type="hidden" value="<%=Model.Id %>" />
        <input id="TagName" class="text" name="TagName" type="text" value="<%= Model.TagName %>" />
	    <input type="submit" class="button" value="Save" />
	</fieldset>
<% Html.EndForm(); %>