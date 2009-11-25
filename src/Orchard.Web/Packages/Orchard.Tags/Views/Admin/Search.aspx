<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<TagsAdminSearchViewModel>" %>
<%@ Import Namespace="Orchard.Models"%>
<%@ Import Namespace="Orchard.Tags.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<% Html.Include("Header"); %>
    <% Html.BeginForm(); %>
    <div class="yui-g">
						<h2 class="separator">List of contents tagged with <%= Model.TagName %></h2>
						<%=Html.ValidationSummary() %>
						<table id="Table1" cellspacing="0" class="roundCorners clearLayout">
							<colgroup>
								<col id="Col1" />
								<col id="Col2" />
							</colgroup>
							<thead>
								<tr>
									<th scope="col">Name</th>
									<th scope="col">Link to the content item</th>
								</tr>
							</thead>
			                <% foreach (var contentItem in Model.Contents) { %>
                            <tr>
                                <td>
                                    <%=contentItem.As<IContentDisplayInfo>().DisplayText%>
                                </td>
				                <td>
				                     <%=Html.ItemDisplayLink(contentItem)%>
				                </td>
                            </tr>
                            <% } %>
				        </table>
	</div>
	<% Html.EndForm(); %>
<% Html.Include("Footer"); %>