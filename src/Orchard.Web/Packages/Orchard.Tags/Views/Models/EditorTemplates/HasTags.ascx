<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<HasTags>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Tags.Models"%>
<h3>Tags</h3>
<% Html.BeginForm("Update", "Home", new { area = "Orchard.Tags" }); %><%= Html.ValidationSummary() %>    <div class="yui-g">
        <h2 class="separator">Edit Tags</h2>       
			     <% 
			     string tags = ""; 
                 foreach (var tag in Model.CurrentTags) {
                        tags += tag.TagName;
                        tags += ",";
                 } %>
			<ol>
			    <li>
			        <input id="Tags" class="inputText inputTextLarge" name="Tags" type="text" value="<%=tags %>" />
			    </li>
			</ol>
	</div>