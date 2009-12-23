<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<MediaFolderEditViewModel>" %>
<%@ Import Namespace="Orchard.Media.Models"%>
<%@ Import Namespace="Orchard.Media.Helpers"%>
<%@ Import Namespace="Orchard.Media.ViewModels"%>
<h2><%=Html.TitleForPage("Manage Folder")%></h2>
<div class="manage"><%=Html.ActionLink("Folder Properties", "EditProperties", new { folderName = Model.FolderName, mediaPath = Model.MediaPath }, new { @class = "button"})%></div>
<p><%=Html.ActionLink("Media Folders", "Index")%> &#62; 
    <%foreach (FolderNavigation navigation in MediaHelpers.GetFolderNavigationHierarchy(Model.MediaPath)) {%>
        <%=Html.ActionLink(navigation.FolderName, "Edit",
                  new {name = navigation.FolderName, mediaPath = navigation.FolderPath})%> &#62;
	    
    <% } %>
    Manage Folder</p>
<% using(Html.BeginFormAntiForgeryPost()) { %>
    <fieldset class="actions bulk">
        <label for="publishActions">Actions: </label>
		<select id="Select1" name="publishActions">
		    <option value="1">Delete</option>
		</select>
		<input class="button roundCorners" type="submit" value="Apply" />
	</fieldset>
	<div class="manage">
	    <%=Html.ActionLink("Add media", "Add", new { folderName = Model.FolderName, mediaPath = Model.MediaPath }, new { @class = "button" })%>
		<%=Html.ActionLink("Add a folder", "Create", new { Model.MediaPath }, new { @class = "button" })%>
    </div>
    <fieldset>
		<table class="items" summary="This is a table of the pages currently available for use in your application.">
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
					<th scope="col">Name</th>
					<th scope="col">Author</th>
					<th scope="col">Last Updated</th>
					<th scope="col">Type</th>
					<th scope="col">Size</th>
				</tr>
			</thead>
			<%foreach (var mediaFile in Model.MediaFiles) {
            %>
            <tr>
                <td>
                    <input type="checkbox" value="true" name="<%= "Checkbox.File." + mediaFile.Name  %>"/>
                    <input type="hidden" value="<%= Model.MediaPath %>" name="<%= mediaFile.Name %>" />
                </td>
                <td>
                    <%=Html.ActionLink(mediaFile.Name, "EditMedia", new { name = mediaFile.Name, 
                                                                          lastUpdated = mediaFile.LastUpdated,
                                                                          size = mediaFile.Size, 
                                                                          folderName = mediaFile.FolderName,
                                                                          mediaPath = Model.MediaPath })%>
                </td>
                <td>Orchard User</td>
                <td><%= mediaFile.LastUpdated %></td>
                <td><%= mediaFile.Type %></td>
                <td><%= mediaFile.Size %></td>
            </tr>
            <%}%>
           <%foreach (var mediaFolder in Model.MediaFolders) {
            %>
            <tr>
                <td>
                    <input type="checkbox" value="true" name="<%= "Checkbox.Folder." + mediaFolder.Name  %>"/>
                    <input type="hidden" value="<%= mediaFolder.MediaPath %>" name="<%= mediaFolder.Name %>" />
                </td>
                <td>
                    <img src="<%=ResolveUrl("~/Packages/Orchard.Media/Content/Admin/images/folder.gif")%>" height="16px" width="16px" class="mediaTypeIcon" alt="Folder" />
                    <%=Html.ActionLink(mediaFolder.Name, "Edit", new { name = mediaFolder.Name,
                                                                       mediaPath = mediaFolder.MediaPath})%>
                </td>
                <td>Orchard User</td>
                <td><%= mediaFolder.LastUpdated %></td>
                <td>Folder</td>
                <td><%= mediaFolder.Size %></td>
            </tr>
            <%}%>
        </table>
    </fieldset>
	<div class="manage">
	    <%=Html.ActionLink("Add media", "Add", new { folderName = Model.FolderName, mediaPath = Model.MediaPath }, new { @class = "button" })%>
		<%=Html.ActionLink("Add a folder", "Create", new { Model.MediaPath }, new { @class = "button" })%>
    </div>
<% } %>