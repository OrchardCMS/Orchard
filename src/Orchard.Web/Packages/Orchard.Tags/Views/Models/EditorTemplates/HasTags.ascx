<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<HasTags>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Tags.Models"%>
<h3>Tags</h3>
<% Html.BeginForm("Update", "Home", new { area = "Orchard.Tags" }); %>
<%= Html.ValidationSummary() %>
    <div class="yui-g">
        <h2 class="separator">Edit Tags</h2>
            <%= Html.Hidden("TaggedContentId", Model.ContentItem.Id) %>
            <%= Html.Hidden("ReturnUrl", Context.Request.Url) %>           
			     <% 
			     string tags = ""; 
                 foreach (var tag in Model.CurrentTags) {
                        tags += tag.TagName;
                        tags += ",";
                 } %>
			<ol>
			    <li>
			        <input id="tags" class="inputText inputTextLarge" name="tags" type="text" value="<%=tags %>" />
			        <input type="submit" class="button" name="submit.Add" value="Add" />
			    </li>
			</ol>
	</div>
<% Html.EndForm(); %>