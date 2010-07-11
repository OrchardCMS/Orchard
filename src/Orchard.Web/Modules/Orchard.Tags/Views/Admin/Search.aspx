<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<TagsAdminSearchViewModel>" %>
<%@ Import Namespace="Orchard.ContentManagement"%>
<%@ Import Namespace="Orchard.Tags.ViewModels"%>
<h1><%: Html.TitleForPage(T("List of contents tagged with {0}", Model.TagName).ToString()) %></h1>
<table class="items">
	<colgroup>
		<col id="Col1" />
		<col id="Col2" />
	</colgroup>
	<thead>
		<tr>
			<th scope="col"><%: T("Name")%></th>
			<th scope="col"><%: T("Link to the content item")%></th>
		</tr>
	</thead>
    <% foreach (var contentItem in Model.Contents) { %>
    <tr>
        <td><%: Html.ItemDisplayText(contentItem) %></td>
        <td><%: Html.ItemDisplayLink(contentItem) %></td>
    </tr>
    <% } %>
</table>