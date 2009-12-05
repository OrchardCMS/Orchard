<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<HasTags>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Tags.Models"%>
<%--<h3>Tags</h3>
<% Html.BeginForm("Edit", "Home", new { area = "Orchard.Tags" }); %>
<%= Html.ValidationSummary() %>
    <div class="yui-g">
            <%= Html.Hidden("TaggedContentId", Model.ContentItem.Id) %>
            <%= Html.Hidden("ReturnUrl", Context.Request.Url) %>
          
			<h3>Add new tags</h3>
			<ol>
			    <li>
			        <input id="NewTagName" class="inputText inputTextLarge" name="NewTagName" type="text" value="" />
			        <input type="submit" class="button" name="submit.Add" value="Add" />
			    </li>
			</ol>
	</div>
<% Html.EndForm(); %>--%>