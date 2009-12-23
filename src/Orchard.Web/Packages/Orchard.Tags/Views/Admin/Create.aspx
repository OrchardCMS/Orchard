<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<TagsAdminCreateViewModel>" %>
<%@ Import Namespace="Orchard.Tags.ViewModels"%>
<h2><%=Html.TitleForPage("Add a Tag") %></h2>
<% using(Html.BeginFormAntiForgeryPost()) { %>
    <%=Html.ValidationSummary() %>
    <fieldset>
        <label for="TagName">Name:</label>
	    <input id="TagName" class="text" name="TagName" type="text" value="<%=Model.TagName%>" />
        <input type="submit" class="button" value="Save" />
    </fieldset>
<% } %>