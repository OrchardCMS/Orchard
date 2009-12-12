<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<CommentsDetailsViewModel>" %>
<%@ Import Namespace="Orchard.Comments.Models"%>
<%@ Import Namespace="Orchard.Comments.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<% Html.Include("AdminHead"); %>
    <h2>Comments for <%= Model.DisplayNameForCommentedItem %></h2>
    <% Html.BeginForm(); %>
		<%=Html.ValidationSummary() %>
        <fieldset class="actions bulk">
            <label for="publishActions">Actions: </label>
            <select id="publishActions" name="<%=Html.NameOf(m => m.Options.BulkAction)%>">
                <%=Html.SelectOption(Model.Options.BulkAction, CommentDetailsBulkAction.None, "Choose action...")%>
                <%=Html.SelectOption(Model.Options.BulkAction, CommentDetailsBulkAction.Delete, "Delete")%>
                <%=Html.SelectOption(Model.Options.BulkAction, CommentDetailsBulkAction.MarkAsSpam, "Mark as Spam")%>
            </select>
            <input class="button roundCorners" type="submit" name="submit.BulkEdit" value="Apply" />
        </fieldset>
        <fieldset class="actions bulk">
            <label for="filterResults">Filter: </label>
            <select id="filterResults" name="<%=Html.NameOf(m => m.Options.Filter)%>">
                <%=Html.SelectOption(Model.Options.Filter, CommentDetailsFilter.All, "All Comments")%>
                <%=Html.SelectOption(Model.Options.Filter, CommentDetailsFilter.Approved, "Approved Comments")%>
                <%=Html.SelectOption(Model.Options.Filter, CommentDetailsFilter.Spam, "Spam")%>
            </select>
            <input class="button roundCorners" type="submit" name="submit.Filter" value="Filter"/>
        </fieldset>
        <div class="manage">
            <% if (Model.CommentsClosedOnItem) {
                %><%=Html.ActionLink("Enable Comments", "Enable", new { commentedItemId = Model.CommentedItemId }, new { @class = "button" })%><%
               } else {
                %><%=Html.ActionLink("Close Comments", "Close", new { commentedItemId = Model.CommentedItemId }, new { @class = "button remove" })%><%
               } %>
        </div>
        <fieldset>
			<table class="items" summary="This is a table of the comments in your application">
				<colgroup>
					<col id="Col1" />
					<col id="Col2" />
					<col id="Col3" />
					<col id="Col4" />
					<col id="Col5" />
					<col id="Col6" />
				</colgroup>
				<thead>
					<tr>
						<th scope="col">&nbsp;&darr;<%-- todo: (heskew) something more appropriate for "this applies to the bulk actions --%></th>
						<th scope="col">Status</th>
						<th scope="col">Author</th>
						<th scope="col">Comment</th>
						<th scope="col">Date</th>
						<th scope="col"></th>
					</tr>
				</thead>
                <%
                    int commentIndex = 0;
                    foreach (var commentEntry in Model.Comments) {
                %>
                <tr>
                    <td>
                        <input type="hidden" value="<%=Model.Comments[commentIndex].Comment.Id%>" name="<%=Html.NameOf(m => m.Comments[commentIndex].Comment.Id)%>"/>
                        <input type="checkbox" value="true" name="<%=Html.NameOf(m => m.Comments[commentIndex].IsChecked)%>"/>
                        <input type="hidden" value="<%= Model.DisplayNameForCommentedItem %>" name="DisplayNameForCommentedtem" />
                        <input type="hidden" value="<%= Model.CommentedItemId %>" name="CommentedItemId" />
                    </td>
                    <td><% if (commentEntry.Comment.Status == CommentStatus.Spam) {%> Spam <% } %>
                    <% else {%> Approved <% } %>
                    </td>
                    <td><%= commentEntry.Comment.UserName %></td>
                    <td>
                    <% if (commentEntry.Comment.CommentText != null) {%>
                        <%= commentEntry.Comment.CommentText.Length > 23 ? commentEntry.Comment.CommentText.Substring(0, 24) : commentEntry.Comment.CommentText %> ...
                    <% } %> 
                    </td>
	                <td><%= commentEntry.Comment.CommentDate.ToLocalTime() %></td>
	                <td>
	                <%=Html.ActionLink("Edit", "Edit", new {commentEntry.Comment.Id}) %> |
	                <%=Html.ActionLink("Delete", "Delete", new {id = commentEntry.Comment.Id, redirectToAction = "Details"}) %>
	                </td>
                </tr>
                <%
                    commentIndex++;
                    } %>
	        </table>
	    </fieldset>
        <div class="manage">
            <% if (Model.CommentsClosedOnItem) {
                %><%=Html.ActionLink("Enable Comments", "Enable", new { commentedItemId = Model.CommentedItemId }, new { @class = "button" })%><%
               } else {
                %><%=Html.ActionLink("Close Comments", "Close", new { commentedItemId = Model.CommentedItemId }, new { @class = "button remove" })%><%
               } %>
        </div>
	<% Html.EndForm(); %>
<% Html.Include("AdminFoot"); %>