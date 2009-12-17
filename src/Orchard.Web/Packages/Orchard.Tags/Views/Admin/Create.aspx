<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<TagsAdminCreateViewModel>" %>
<%@ Import Namespace="Orchard.Tags.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<h2>Add a Tag</h2>
<%-- todo: (heskew) change all of the explicit begin/end forms to using blocks --%>
<% Html.BeginForm(); %>
    <%= Html.ValidationSummary() %>
    <fieldset>
        <label for="TagName">Name:</label>
	    <input id="TagName" class="text" name="TagName" type="text" value="<%= Model.TagName %>" />
        <input type="submit" class="button" value="Save" />
    </fieldset>
<% Html.EndForm(); %>