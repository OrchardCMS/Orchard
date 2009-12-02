<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<HasComments>" %>
<%@ Import Namespace="Orchard.Comments.Models"%>
<h3><%= Model.Comments.Count() %> Comments</h3>
<ol>
    <% foreach (var comment in Model.Comments) {%>
    <li>
        <%= comment.CommentText %>
    </li>
    <li>
        Posted by <%= comment.UserName %> on <%= comment.CommentDate %>
    </li>
    <hr />
    <% } %>
</ol>
<% if (Model.Closed) { %>
<p>Comments have been disabled for this content.</p>
<%= Html.ActionLink("Enable Comments for this content", "Enable", new { Area="Orchard.Comments", Controller="Admin", returnUrl = Context.Request.Url, commentedItemId = Model.ContentItem.Id })%>
<% } else { %>
<%= Html.ActionLink("Close Comments for this content", "Close", new { Area="Orchard.Comments", Controller="Admin", returnUrl = Context.Request.Url, commentedItemId = Model.ContentItem.Id })%>
<% Html.BeginForm("Create", "Admin", new { area = "Orchard.Comments" }); %>
    <%= Html.ValidationSummary() %>
                    <div class="yui-g">
						<h2 class="separator">Add a Comment</h2>
						<h3>Information</h3>
						<ol>
						<li>
						<%= Html.Hidden("CommentedOn", Model.ContentItem.Id) %>
						<%= Html.Hidden("ReturnUrl", Context.Request.Url) %>
					    <label for="Name">Name:</label>
						<input id="Text1" class="inputText inputTextLarge" name="Name" type="text" />
						</li>
						<li>
				        <label for="Email">Email:</label>
					    <input id="Email" class="inputText inputTextLarge" name="Email" type="text" />					
						</li>
                        <li>
                        <label for="SiteName">SiteName:</label>
					    <input id="SiteName" class="inputText inputTextLarge" name="SiteName" type="text" />
                        </li>
                        <li>
                        <label for="CommentText">Leave a comment</label>
					    <textarea id="CommentText" rows="10" cols="30" name="CommentText">
					    </textarea>
                        </li>
					    <li>
					    <input type="submit" class="button" value="Submit Comment" />
					    </li>
					    </ol>
					</div>
	<% Html.EndForm(); %>
<% } %>