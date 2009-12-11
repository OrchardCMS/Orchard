<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<TagsAdminSearchViewModel>" %>
<%@ Import Namespace="Orchard.Models"%>
<%@ Import Namespace="Orchard.Tags.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<% Html.Include("AdminHead"); %>
	<h2>List of contents tagged with <%=Model.TagName %></h2>
    <% using(Html.BeginForm()) { %>
		<%=Html.ValidationSummary() %>
		<fieldset>
			<table cellspacing="0" class="roundCorners clearLayout">
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
                    <td><%=Html.ItemDisplayText(contentItem)%></td>
	                <td><%=Html.ItemDisplayLink(contentItem)%></td>
                </tr>
                <% } %>
	        </table>
        </fieldset>
	<% } %>
<% Html.Include("AdminFoot"); %>