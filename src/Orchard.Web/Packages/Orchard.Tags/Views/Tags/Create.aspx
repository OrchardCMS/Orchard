<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<TagsCreateViewModel>" %>
<%@ Import Namespace="Orchard.Tags.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<% Html.Include("Header"); %>
    <% Html.BeginForm(); %>
    <%= Html.ValidationSummary() %>
                    <div class="yui-g">
						<h2 class="separator">Add a Tag</h2>
					    <label for="TagName">Name:</label>
						<input id="TagName" class="inputText inputTextLarge" name="TagName" type="text" value="<%= Model.TagName %>" />
					    <input type="submit" class="button" value="Save" />
					</div>
	<% Html.EndForm(); %>
<% Html.Include("Footer"); %>