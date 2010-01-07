<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<CommentsDetailsViewModel>" %>
<%@ Import Namespace="Orchard.Comments.Models"%>
<%@ Import Namespace="Orchard.Comments.ViewModels"%>
<h1><%=Html.TitleForPage(T("Comments for {0}", Model.DisplayNameForCommentedItem).ToString()) %></h1>
<% using(Html.BeginFormAntiForgeryPost()) { %>
	<%=Html.ValidationSummary() %>
    <fieldset class="actions bulk">
        <label for="publishActions"><%=_Encoded("Actions:") %></label>
        <select id="publishActions" name="<%=Html.NameOf(m => m.Options.BulkAction)%>">
            <%=Html.SelectOption(Model.Options.BulkAction, CommentDetailsBulkAction.None, _Encoded("Choose action...").ToString())%>
            <%=Html.SelectOption(Model.Options.BulkAction, CommentDetailsBulkAction.Delete, _Encoded("Delete").ToString())%>
            <%=Html.SelectOption(Model.Options.BulkAction, CommentDetailsBulkAction.MarkAsSpam, _Encoded("Mark as Spam").ToString())%>
        </select>
        <input class="button" type="submit" name="submit.BulkEdit" value="<%=_Encoded("Apply") %>" />
    </fieldset>
    <fieldset class="actions bulk">
        <label for="filterResults"><%=_Encoded("Filter:")%></label>
        <select id="filterResults" name="<%=Html.NameOf(m => m.Options.Filter)%>">
            <%=Html.SelectOption(Model.Options.Filter, CommentDetailsFilter.All, _Encoded("All Comments").ToString())%>
            <%=Html.SelectOption(Model.Options.Filter, CommentDetailsFilter.Approved, _Encoded("Approved Comments").ToString())%>
            <%=Html.SelectOption(Model.Options.Filter, CommentDetailsFilter.Spam, _Encoded("Spam").ToString())%>
        </select>
        <input class="button" type="submit" name="submit.Filter" value="<%=_Encoded("Filter") %>"/>
    </fieldset>
    <div class="manage">
        <% if (Model.CommentsClosedOnItem) {
            %><%=Html.ActionLink(T("Enable Comments").ToString(), "Enable", new { commentedItemId = Model.CommentedItemId }, new { @class = "button" })%><%
           } else {
            %><%=Html.ActionLink(T("Close Comments").ToString(), "Close", new { commentedItemId = Model.CommentedItemId }, new { @class = "button remove" })%><%
           } %>
    </div>
    <fieldset>
		<table class="items" summary="<%=_Encoded("This is a table of the comments for the content item") %>">
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
				    <th scope="col"><%=_Encoded("Status") %></th>
				    <th scope="col"><%=_Encoded("Author") %></th>
				    <th scope="col"><%=_Encoded("Comment") %></th>
				    <th scope="col"><%=_Encoded("Date") %></th>
					<th scope="col"></th>
				</tr>
			</thead>
            <%
                int commentIndex = 0;
                foreach (var commentEntry in Model.Comments) {
                    var ci = commentIndex;
            %>
            <tr>
                <td>
                    <input type="hidden" value="<%=Model.Comments[commentIndex].Comment.Id %>" name="<%=Html.NameOf(m => m.Comments[ci].Comment.Id) %>"/>
                    <input type="checkbox" value="true" name="<%=Html.NameOf(m => m.Comments[ci].IsChecked) %>"/>
                    <input type="hidden" value="<%=Model.DisplayNameForCommentedItem %>" name="DisplayNameForCommentedtem" />
                    <input type="hidden" value="<%=Model.CommentedItemId %>" name="CommentedItemId" />
                </td>
                <td><% if (commentEntry.Comment.Status == CommentStatus.Spam) { %><%=_Encoded("Spam") %><% } else { %><%=_Encoded("Approved") %><% } %></td>
                <td><%=Html.Encode(commentEntry.Comment.UserName) %></td>
                <td>
                <% if (commentEntry.Comment.CommentText != null) {%>
                    <%=Html.Encode(commentEntry.Comment.CommentText.Length > 23 ? commentEntry.Comment.CommentText.Substring(0, 24) : commentEntry.Comment.CommentText) %><%=_Encoded(" ...") %>
                <% } %> 
                </td>
                <td><%=commentEntry.Comment.CommentDate.ToLocalTime() %></td>
                <td>
                    <%=Html.ActionLink(T("Edit").ToString(), "Edit", new {commentEntry.Comment.Id}) %> |
                    <%=Html.ActionLink(T("Delete").ToString(), "Delete", new {id = commentEntry.Comment.Id, redirectToAction = "Details"}) %>
                </td>
            </tr>
            <%
                commentIndex++;
                } %>
        </table>
    </fieldset>
    <div class="manage">
        <% if (Model.CommentsClosedOnItem) {
            %><%=Html.ActionLink(T("Enable Comments").ToString(), "Enable", new { commentedItemId = Model.CommentedItemId }, new { @class = "button" })%><%
           } else {
            %><%=Html.ActionLink(T("Close Comments").ToString(), "Close", new { commentedItemId = Model.CommentedItemId }, new { @class = "button remove" })%><%
           } %>
    </div>
<% } %>