<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<MediaFolderEditViewModel>" %>
<%@ Import Namespace="Orchard.Media.Models"%>
<%@ Import Namespace="Orchard.Media.Helpers"%>
<%@ Import Namespace="Orchard.Media.ViewModels"%>
<h1><%=Html.TitleForPage(T("Manage Folder").ToString())%></h1>


<%--<div class="manage"><%=Html.ActionLink(T("Folder Properties").ToString(), "EditProperties", new { folderName = Model.FolderName, mediaPath = Model.MediaPath }, new { @class = "button"})%></div>--%>

<div class="breadCrumbs">
<p><%=Html.ActionLink(T("Media Folders").ToString(), "Index")%> &#62; 
    <%foreach (FolderNavigation navigation in MediaHelpers.GetFolderNavigationHierarchy(Model.MediaPath)) {%>
        <%=Html.ActionLink(navigation.FolderName, "Edit",
                  new {name = navigation.FolderName, mediaPath = navigation.FolderPath})%> &#62;
	    
    <% } %>
    <%=_Encoded("Manage Folder")%></p>
</div> 
<div class="folderProperties">
<p><%=Html.ActionLink(T("Folder Properties").ToString(), "EditProperties", new { folderName = Model.FolderName, mediaPath = Model.MediaPath })%></p>
</div> 
<div class="clearBoth"></div>   
    
    
<% using(Html.BeginFormAntiForgeryPost()) { %>
    <fieldset class="actions bulk">
        <label for="publishActions"><%=_Encoded("Actions:")%></label>
		<select id="Select1" name="publishActions">
		    <option value="1"><%=_Encoded("Remove")%></option>
		</select>
		<input class="button" type="submit" value="<%=_Encoded("Apply") %>" />
	</fieldset>
	<div class="manage">
	    <%=Html.ActionLink(T("Add media").ToString(), "Add", new { folderName = Model.FolderName, mediaPath = Model.MediaPath }, new { @class = "button primaryAction" })%>
		<%=Html.ActionLink(T("Add a folder").ToString(), "Create", new { Model.MediaPath }, new { @class = "button" })%>
    </div>
    <fieldset>
		<table class="items" summary="<%=_Encoded("This is a table of the pages currently available for use in your application.") %>">
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
					<th scope="col"><%=_Encoded("Name") %></th>
					<th scope="col"><%=_Encoded("Author") %></th>
					<th scope="col"><%=_Encoded("Last Updated") %></th>
					<th scope="col"><%=_Encoded("Type") %></th>
					<th scope="col"><%=_Encoded("Size") %></th>
				</tr>
			</thead>
			<%foreach (var mediaFile in Model.MediaFiles) {
            %>
            <tr>
                <td>
                    <input type="checkbox" value="true" name="<%=_Encoded("Checkbox.File.{0}", mediaFile.Name)  %>"/>
                    <input type="hidden" value="<%=_Encoded(Model.MediaPath) %>" name="<%=_Encoded(mediaFile.Name) %>" />
                </td>
                <td>
                    <%=Html.ActionLink(mediaFile.Name, "EditMedia", new { name = mediaFile.Name, 
                                                                          lastUpdated = mediaFile.LastUpdated,
                                                                          size = mediaFile.Size, 
                                                                          folderName = mediaFile.FolderName,
                                                                          mediaPath = Model.MediaPath })%>
                </td>
                <td><%=_Encoded("Orchard User")%></td>
                <td><%=mediaFile.LastUpdated %></td>
                <td><%=Html.Encode(mediaFile.Type) %></td>
                <td><%=mediaFile.Size %></td>
            </tr>
            <%}%>
           <%foreach (var mediaFolder in Model.MediaFolders) {
            %>
            <tr>
                <td>
                    <input type="checkbox" value="true" name="<%=_Encoded("Checkbox.Folder.{0}", mediaFolder.Name)  %>"/>
                    <input type="hidden" value="<%=Html.Encode(mediaFolder.MediaPath) %>" name="<%=Html.Encode(mediaFolder.Name) %>" />
                </td>
                <td>
                    <img src="<%=ResolveUrl("~/Modules/Orchard.Media/Content/Admin/images/folder.gif")%>" height="16" width="16" class="mediaTypeIcon" alt="<%=_Encoded("Folder") %>" />
                    <%=Html.ActionLink(mediaFolder.Name, "Edit", new { name = mediaFolder.Name, mediaPath = mediaFolder.MediaPath})%>
                </td>
                <td><%=_Encoded("Orchard User")%></td>
                <td><%=mediaFolder.LastUpdated %></td>
                <td><%=_Encoded("Folder")%></td>
                <td><%=mediaFolder.Size %></td>
            </tr>
            <%}%>
        </table>
    </fieldset>
	<div class="manage">
	    <%=Html.ActionLink(T("Add media").ToString(), "Add", new { folderName = Model.FolderName, mediaPath = Model.MediaPath }, new { @class = "button primaryAction" })%>
		<%=Html.ActionLink(T("Add a folder").ToString(), "Create", new { Model.MediaPath }, new { @class = "button" })%>
    </div>
<% } %>