<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<HasTags>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Tags.Models"%>
<h3>Tags</h3>
<% Html.BeginForm("Edit", "Tags", new { area = "Orchard.Tags" }); %>
<%= Html.ValidationSummary() %>
    <div class="yui-g">
        <h2 class="separator">Edit Tags</h2>
            <ol>
                 <%= Html.Hidden("ReturnUrl", Context.Request.Url) %>
                 <% foreach (var tag in Model.AllTags) { %>
                    <li>
                        <label for"<%= tag.TagName %>"><%= tag.TagName %>:</label>
                        <input type="checkbox" value="true" name="<%= tag.TagName %>"/>
                    </li>
                  <% } %>
			    <li>
			        <input type="submit" class="button" value="Save" />
			    </li>
			</ol>
	</div>
<% Html.EndForm(); %>