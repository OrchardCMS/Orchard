<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<HasTags>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Tags.Models"%>
<h3>Tags</h3>
<% Html.BeginForm("Edit", "Home", new { area = "Orchard.Tags" }); %>
<%= Html.ValidationSummary() %>
    <div class="yui-g">
        <h2 class="separator">Edit Tags</h2>
            <%= Html.Hidden("TaggedContentId", Model.ContentItem.Id) %>
            <%= Html.Hidden("ReturnUrl", Context.Request.Url) %>
            <h3>Choose from existing tags</h3>
            <ol>
                 <% foreach (var tag in Model.AllTags) { %>
                    <li>
                        <label for"<%= tag.TagName %>"><%= tag.TagName %>:</label>
                    <% if (Model.CurrentTags.Contains(tag)) {%>
                        <input type="checkbox" value="true" checked="checked" name="Checkbox.<%=tag.Id%>"/>
                    <% } else {%>
                        <input type="checkbox" value="true" name="Checkbox.<%=tag.Id%>"/>
                    <% } %>
                    </li>
                  <% } %>
			    <li>
			        <input type="submit" class="button" name="submit.Save" value="Save" />
			    </li>
			</ol>            
			<h3>Or add a new tag</h3>
			<ol>
			    <li>
			        <input id="NewTagName" class="inputText inputTextLarge" name="NewTagName" type="text" value="" />
			        <input type="submit" class="button" name="submit.Add" value="Add" />
			    </li>
			</ol>
	</div>
<% Html.EndForm(); %>