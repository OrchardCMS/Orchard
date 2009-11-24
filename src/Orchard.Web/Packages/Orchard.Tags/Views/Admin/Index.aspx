<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<TagsAdminIndexViewModel>" %>
<%@ Import Namespace="Orchard.Tags.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<% Html.Include("Header"); %>
    <% Html.BeginForm(); %>
    <div class="yui-g">
						<h2 class="separator">Manage Comments</h2>
						<%=Html.ValidationSummary() %>
						<ol class="horizontal actions floatLeft">
                        <li>
                            <label class="floatLeft" for="publishActions"> Actions:</label>
                            <select id="publishActions" name="<%=Html.NameOf(m => m.BulkAction)%>">
                                <%=Html.SelectOption(Model.BulkAction, TagAdminIndexBulkAction.None, "Choose action...")%>
                                <%=Html.SelectOption(Model.BulkAction, TagAdminIndexBulkAction.Delete, "Delete")%>
                            </select>
                        </li>
                        <li>
                        <input class="button roundCorners" type="submit" name="submit" value="Apply" />
                        </li>
                        </ol>
                        <span class="filterActions">
						    <%=Html.ActionLink("Add a new tag", "Create") %>
						</span>
						<table id="Table1" cellspacing="0" class="roundCorners clearLayout" summary="This is a table of the tags in your application">
							<colgroup>
								<col id="Col1" />
								<col id="Col2" />
								<col id="Col3" />
							</colgroup>
							<thead>
								<tr>
									<th scope="col"><%--<input type="checkbox" value="" />--%></th>
									<th scope="col">Name</th>
									<th scope="col"></th>
								</tr>
							</thead>
			                <%
			                    int tagIndex = 0;
			                    foreach (var tagEntry in Model.Tags) {
                            %>
                            <tr>
                                <td>
                                    <input type="hidden" value="<%=Model.Tags[tagIndex].Tag.Id%>" name="<%=Html.NameOf(m => m.Tags[tagIndex].Tag.Id)%>"/>
                                    <input type="checkbox" value="true" name="<%=Html.NameOf(m => m.Tags[tagIndex].IsChecked)%>"/>
                                </td>
                                <td>
                                    <%= tagEntry.Tag.TagName %>
                                </td>
				                <td>
				                <%=Html.ActionLink("Edit", "Edit", new {id = tagEntry.Tag.Id}, new {@class="floatRight topSpacer"}) %>
				                </td>
                            </tr>
                            <% tagIndex++; } %>
				        </table>
	</div>
	<% Html.EndForm(); %>
<% Html.Include("Footer"); %>