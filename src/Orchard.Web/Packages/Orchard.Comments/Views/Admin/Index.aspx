<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<CommentsIndexViewModel>" %>
<%@ Import Namespace="Orchard.Comments.Models"%>
<%@ Import Namespace="Orchard.Comments.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<% Html.Include("Header"); %>
    <% Html.BeginForm(); %>
    <div class="yui-g">
						<h2 class="separator">Manage Comments</h2>
						<%=Html.ValidationSummary() %>
						<ol class="horizontal actions floatLeft">
                        <li>
                            <label class="floatLeft" for="publishActions"> Actions:</label>
                            <select id="publishActions" name="<%=Html.NameOf(m => m.Options.BulkAction)%>">
                                <%=Html.SelectOption(Model.Options.BulkAction, CommentIndexBulkAction.None, "Choose action...")%>
                                <%=Html.SelectOption(Model.Options.BulkAction, CommentIndexBulkAction.Delete, "Delete")%>
                                <%=Html.SelectOption(Model.Options.BulkAction, CommentIndexBulkAction.MarkAsSpam, "Mark as Spam")%>
                            </select>
                        </li>
                        <li>
                        <input class="button roundCorners" type="submit" name="submit.BulkEdit" value="Apply" />
                        </li>
                        </ol>
                        <ol class="horizontal actions">
                        <li>
                            <label class="floatLeft" for="filterResults"></label>
                            <select id="filterResults" name="<%=Html.NameOf(m => m.Options.Filter)%>">
                                <%=Html.SelectOption(Model.Options.Filter, CommentIndexFilter.All, "All Comments")%>
                                <%=Html.SelectOption(Model.Options.Filter, CommentIndexFilter.Approved, "Approved Comments")%>
                                <%=Html.SelectOption(Model.Options.Filter, CommentIndexFilter.Spam, "Spam")%>
                            </select>
                        </li>
                        <li>
                        <input class="button roundCorners" type="submit" name="submit.Filter" value="Filter"/>
                        </li>
                        </ol>
						<table id="Table1" cellspacing="0" class="roundCorners clearLayout" summary="This is a table of the comments in your application">
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
									<th scope="col"><%--<input type="checkbox" value="" />--%></th>
									<th scope="col">Status</th>
									<th scope="col">Author</th>
									<th scope="col">Comment</th>
									<th scope="col">Date</th>
									<th scope="col">Commented On</th>
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
                                </td>
                                <td><% if (commentEntry.Comment.Status == CommentStatus.Spam) {%> Spam <% } %>
                                <% else {%> Approved <% } %>
                                </td>
                                <td><%= commentEntry.Comment.UserName %></td>
			                    <td>
			                    <% if (commentEntry.Comment.CommentText != null) {%>
			                        <%= commentEntry.Comment.CommentText.Length > 23 ? commentEntry.Comment.CommentText.Substring(0, 24) : commentEntry.Comment.CommentText %>
			                    <% } %> 
			                    </td>
				                <td><%= commentEntry.Comment.CommentDate %></td>
				                <td><%=Html.ActionLink(commentEntry.CommentedOn, "Details", new {}, new {@class="floatRight topSpacer"}) %></td>
                            </tr>
                            <%
                                commentIndex++;
                                } %>
				        </table>
	</div>
	<% Html.EndForm(); %>
<% Html.Include("Footer"); %>