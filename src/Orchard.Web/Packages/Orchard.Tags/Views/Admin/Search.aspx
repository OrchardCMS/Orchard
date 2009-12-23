<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<TagsAdminSearchViewModel>" %>
<%@ Import Namespace="Orchard.ContentManagement"%>
<%@ Import Namespace="Orchard.Tags.ViewModels"%>
<h2><%=Html.TitleForPage(string.Format("List of contents tagged with {0}", Model.TagName)) %></h2>
<% using(Html.BeginFormAntiForgeryPost()) { %>
	<%=Html.ValidationSummary() %>
	<fieldset>
		<table class="items">
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