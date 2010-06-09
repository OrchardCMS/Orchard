<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<CommentsDetailsViewModel>" %>
<%@ Import Namespace="Orchard.Comments.Models"%>
<%@ Import Namespace="Orchard.Comments.ViewModels"%>
<h1><%: Html.TitleForPage(T("Comments for {0}", Model.DisplayNameForCommentedItem).ToString()) %></h1>
<div class="manage"><%
    if (Model.CommentsClosedOnItem) {
        using (Html.BeginFormAntiForgeryPost(Url.Action("Enable", new { commentedItemId = Model.CommentedItemId }), FormMethod.Post, new { @class = "inline" })) { %>
    <fieldset>
        <button type="submit" title="<%: T("Enable Comments") %>"><%: T("Enable Comments")%></button>
    </fieldset><%
        }
    } else {
        using (Html.BeginFormAntiForgeryPost(Url.Action("Close", new { commentedItemId = Model.CommentedItemId }), FormMethod.Post, new { @class = "inline" })) { %>
    <fieldset>
        <button type="submit" class="remove" title="<%: T("Close Comments") %>"><%: T("Close Comments")%></button>
    </fieldset><%
        }
    } %>
</div>
<% using(Html.BeginFormAntiForgeryPost()) { %>
	<%: Html.ValidationSummary() %>
    <fieldset class="actions bulk">
        <label for="publishActions"><%: T("Actions:") %></label>
        <select id="publishActions" name="<%=Html.NameOf(m => m.Options.BulkAction)%>">
            <%: Html.SelectOption(Model.Options.BulkAction, CommentDetailsBulkAction.None, _Encoded("Choose action...").ToString())%>
            <%: Html.SelectOption(Model.Options.BulkAction, CommentDetailsBulkAction.Approve, _Encoded("Approve").ToString())%>
            <%: Html.SelectOption(Model.Options.BulkAction, CommentDetailsBulkAction.Pend, _Encoded("Pend").ToString())%>
            <%: Html.SelectOption(Model.Options.BulkAction, CommentDetailsBulkAction.MarkAsSpam, _Encoded("Mark as Spam").ToString())%>
            <%: Html.SelectOption(Model.Options.BulkAction, CommentDetailsBulkAction.Delete, _Encoded("Remove").ToString())%>
        </select>
        <input class="button" type="submit" name="submit.BulkEdit" value="<%: T("Apply") %>" />
    </fieldset>
    <fieldset class="actions bulk">
        <label for="filterResults"><%: T("Filter:")%></label>
        <select id="filterResults" name="<%=Html.NameOf(m => m.Options.Filter)%>">
            <%: Html.SelectOption(Model.Options.Filter, CommentDetailsFilter.All, _Encoded("All Comments").ToString())%>
            <%: Html.SelectOption(Model.Options.Filter, CommentDetailsFilter.Approved, _Encoded("Approved Comments").ToString())%>
            <%: Html.SelectOption(Model.Options.Filter, CommentDetailsFilter.Pending, _Encoded("Pending Comments").ToString())%>
            <%: Html.SelectOption(Model.Options.Filter, CommentDetailsFilter.Spam, _Encoded("Spam").ToString())%>
        </select>
        <input class="button" type="submit" name="submit.Filter" value="<%: T("Apply") %>"/>
    </fieldset>
    <fieldset>
		<table class="items" summary="<%: T("This is a table of the comments for the content item") %>">
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
				    <th scope="col"><%: T("Status") %></th>
				    <th scope="col"><%: T("Author") %></th>
				    <th scope="col"><%: T("Comment") %></th>
				    <th scope="col"><%: T("Date") %></th>
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
                <td>
                <% if (commentEntry.Comment.Status == CommentStatus.Spam) { %><%: T("Spam") %><% } 
                       else if (commentEntry.Comment.Status == CommentStatus.Pending) { %><%: T("Pending") %><% } 
                       else { %><%: T("Approved") %><% } %>
                </td>
                <td><%: commentEntry.Comment.UserName %></td>
                <td>
                <% if (commentEntry.Comment.CommentText != null) {%>
                    <%: commentEntry.Comment.CommentText.Length > 23 ? commentEntry.Comment.CommentText.Substring(0, 24) : commentEntry.Comment.CommentText %><%: T(" ...") %>
                <% } %> 
                </td>
                <td><%=Html.DateTime(commentEntry.Comment.CommentDateUtc.GetValueOrDefault()) %></td>
                <td>
                    <ul class="actions">
                        <li class="construct">
                            <a href="<%=Url.Action("Edit", new {commentEntry.Comment.Id}) %>" class="ibutton edit" title="<%: T("Edit Comment")%>"><%: T("Edit Comment")%></a>
                        </li>
                        <li class="destruct">
<%-- a form in a form doesn't quite work                            <% using (Html.BeginFormAntiForgeryPost(Url.Action("Delete", new {id = commentEntry.Comment.Id, redirectToAction = "Details"}), FormMethod.Post, new { @class = "inline" })) { %>
                                <fieldset>
                                    <button type="submit" class="ibutton remove" title="<%: T("Remove Comment") %>"><%: T("Remove Comment") %></button>
                                </fieldset><%
                            } %>
--%>                        </li>
                    </ul>
                </td>
            </tr>
            <%
                commentIndex++;
                } %>
        </table>
    </fieldset>
<% } %>
<div class="manage"><%
    if (Model.CommentsClosedOnItem) {
        using (Html.BeginFormAntiForgeryPost(Url.Action("Enable", new { commentedItemId = Model.CommentedItemId }), FormMethod.Post, new { @class = "inline" })) { %>
    <fieldset>
        <button type="submit" title="<%: T("Enable Comments") %>"><%: T("Enable Comments")%></button>
    </fieldset><%
        }
    } else {
        using (Html.BeginFormAntiForgeryPost(Url.Action("Close", new { commentedItemId = Model.CommentedItemId }), FormMethod.Post, new { @class = "inline" })) { %>
    <fieldset>
        <button type="submit" class="remove" title="<%: T("Close Comments") %>"><%: T("Close Comments")%></button>
    </fieldset><%
        }
    } %>
</div>