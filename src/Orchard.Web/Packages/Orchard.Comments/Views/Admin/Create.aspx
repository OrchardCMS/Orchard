<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<CommentsCreateViewModel>" %>
<%@ Import Namespace="Orchard.Comments.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<% Html.Include("Header"); %>
    <% Html.BeginForm(); %>
    <%= Html.ValidationSummary() %>
                    <div class="yui-g">
						<h2 class="separator">Add a Comment</h2>
						<h3>Information</h3>
						<ol>
						<li>
					    <label for="Name">Name:</label>
						<input id="Text1" class="inputText inputTextLarge" name="Name" type="text" value="<%= Model.Name %>" />
						</li>
						<li>
				        <label for="Email">Email:</label>
					    <input id="Email" class="inputText inputTextLarge" name="Email" type="text" value="<%= Model.Email%>" />					
						</li>
                        <li>
                        <label for="SiteName">SiteName:</label>
					    <input id="SiteName" class="inputText inputTextLarge" name="SiteName" type="text" value="<%= Model.SiteName %>" />
                        </li>
                        <li>
                        <label for="CommentText">Leave a comment</label>
					    <textarea id="CommentText" rows="10" cols="30" name="CommentText">
					    <%= Model.CommentText %>
					    </textarea>
                        </li>
					    <li>
					    <input type="submit" class="button" value="Save" />
					    </li>
					    </ol>
					</div>
	<% Html.EndForm(); %>
<% Html.Include("Footer"); %>