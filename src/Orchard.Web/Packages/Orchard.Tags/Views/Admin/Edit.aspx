<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<TagsAdminEditViewModel>" %>
<%@ Import Namespace="Orchard.Tags.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<% Html.Include("Header"); %>
    <% Html.BeginForm(); %>
    <%= Html.ValidationSummary() %>
                    <div class="yui-g">
						<h2 class="separator">Edit a Tag</h2>
						<h3>Information</h3>
						<ol>
						<li>
					    <label for="Name">Name:</label>
					    <input id="Id" name="Id" type="hidden" value="<%=Model.Id %>" />
						<input id="TagName" class="inputText inputTextLarge" name="TagName" type="text" value="<%= Model.TagName %>" />
						</li>
					    <li>
					    <input type="submit" class="button" value="Save" />
					    </li>
					    </ol>
					</div>
	<% Html.EndForm(); %>
<% Html.Include("Footer"); %>