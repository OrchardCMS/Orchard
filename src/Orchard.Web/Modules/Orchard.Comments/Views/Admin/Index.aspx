<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<CommentsIndexViewModel>" %>
<%@ Import Namespace="Orchard.Comments.Models"%>
<%@ Import Namespace="Orchard.Comments.ViewModels"%>
<h1><%: Html.TitleForPage(T("Manage Comments").ToString())%></h1>
<% using(Html.BeginFormAntiForgeryPost()) { %>
	<%: Html.ValidationSummary() %>
    <fieldset class="actions bulk">
        <label for="publishActions"><%: T("Actions:") %></label>
        <select id="publishActions" name="<%=Html.NameOf(m => m.Options.BulkAction)%>">
            <%: Html.SelectOption(Model.Options.BulkAction, CommentIndexBulkAction.None, T("Choose action...").ToString()) %>
            <%: Html.SelectOption(Model.Options.BulkAction, CommentIndexBulkAction.Approve, T("Approve")) %>
            <%: Html.SelectOption(Model.Options.BulkAction, CommentIndexBulkAction.Pend, T("Pend")) %>
            <%: Html.SelectOption(Model.Options.BulkAction, CommentIndexBulkAction.MarkAsSpam, T("Mark as Spam")) %>
            <%: Html.SelectOption(Model.Options.BulkAction, CommentIndexBulkAction.Delete, T("Remove"))%>
        </select>
        <input class="button" type="submit" name="submit.BulkEdit" value="<%: T("Apply") %>" />
    </fieldset>
    <fieldset class="actions bulk">
        <label for="filterResults"><%: T("Filter:")%></label>
        <select id="filterResults" name="<%=Html.NameOf(m => m.Options.Filter)%>">
            <%: Html.SelectOption(Model.Options.Filter, CommentIndexFilter.All, T("All Comments")) %>
            <%: Html.SelectOption(Model.Options.Filter, CommentIndexFilter.Approved, T("Approved Comments")) %>
            <%: Html.SelectOption(Model.Options.Filter, CommentIndexFilter.Pending, T("Pending Comments")) %>
            <%: Html.SelectOption(Model.Options.Filter, CommentIndexFilter.Spam, T("Spam"))%>
        </select>
        <input class="button" type="submit" name="submit.Filter" value="<%: T("Apply") %>"/>
    </fieldset>
    <fieldset>
	    <table class="items" summary="<%: T("This is a table of the comments in your application") %>">
		    <colgroup>
			    <col id="Col1" />
			    <col id="Col2" />
			    <col id="Col3" />
			    <col id="Col4" />
			    <col id="Col5" />
			    <col id="Col6" />
			    <col id="Col7" />
		    </colgroup>
		    <thead>
			    <tr>
				    <th scope="col">&nbsp;&darr;<%-- todo: (heskew) something more appropriate for "this applies to the bulk actions --%></th>
				    <th scope="col"><%: T("Status") %></th>
				    <th scope="col"><%: T("Author") %></th>
				    <th scope="col"><%: T("Comment") %></th>
				    <th scope="col"><%: T("Date") %></th>
				    <th scope="col"><%: T("Commented On") %></th>
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
                </td>
                <td><% if (commentEntry.Comment.Status == CommentStatus.Spam) { %><%: T("Spam") %><% } 
                       else if (commentEntry.Comment.Status == CommentStatus.Pending) { %><%: T("Pending") %><% } 
                       else { %><%: T("Approved") %><% } %></td>
                <td><%: commentEntry.Comment.UserName %></td>
                <td>
                <% if (commentEntry.Comment.CommentText != null) {%>
                    <%-- todo: (heskew) same text processing comment as on the public display, also need to use the ellipsis character instead of ... --%>
                    <%: commentEntry.Comment.CommentText.Length > 23 ? commentEntry.Comment.CommentText.Substring(0, 24) : commentEntry.Comment.CommentText %><%: T(" ...") %>
                <% } %> 
                </td>
                <td><%=Html.DateTime(commentEntry.Comment.CommentDateUtc.GetValueOrDefault()) %></td>
                <td><%: Html.ActionLink(commentEntry.CommentedOn, "Details", new { id = commentEntry.Comment.CommentedOn }) %></td>
                <td>
                    <ul class="actions">
                        <li class="construct">
                            <a href="<%=Url.Action("Edit", new {commentEntry.Comment.Id}) %>" title="<%: T("Edit")%>"><%: T("Edit")%></a>
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