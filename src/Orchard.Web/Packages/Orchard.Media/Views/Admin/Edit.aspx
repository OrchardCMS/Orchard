<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<MediaFolderEditViewModel>" %>
<%@ Import Namespace="Orchard.Media.Models"%>
<%@ Import Namespace="Orchard.Media.Helpers"%>
<%@ Import Namespace="Orchard.Media.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Manage Media Folder</title>
    <% Html.Include("Head"); %>
</head>
<body>
    <% Html.Include("Header"); %>
    <% Html.BeginForm(); %>
    <div class="yui-g">
						<h2 class="separator">Manage Folder</h2>
						<span class="filterActions">
						    <%=Html.ActionLink("Folder Properties", "EditProperties", new { folderName = Model.FolderName, mediaPath = Model.MediaPath })%>
						</span>
						<p class="bottomSpacer">
						<%=Html.ActionLink("Media Folders", "Index")%> &#62; 
					    <%foreach (FolderNavigation navigation in MediaHelpers.GetFolderNavigationHierarchy(Model.MediaPath)) {%>
						    <%=Html.ActionLink(navigation.FolderName, "Edit",
                                      new {name = navigation.FolderName, mediaPath = navigation.FolderPath})%> &#62;
						    
						<% } %>
						Manage Folder</p>
						<ol class="horizontal actions floatLeft">
							<li><label class="floatLeft" for="bulkActions">Actions:</label>
							<select id="Select1" name="publishActions">
							<option value="1">Delete</option>
							</select> </li>
							<li>
							<input class="button roundCorners" type="submit" value="Apply" />
							</li>
						</ol>
						<span class="filterActions">
						    <%=Html.ActionLink("Add media", "Add", new { folderName = Model.FolderName, mediaPath = Model.MediaPath })%>
						    |
						    <%=Html.ActionLink("Add a new folder", "Create", new { Model.MediaPath }) %>
						</span>
						<table id="Table1" cellspacing="0" class="roundCorners clearLayout" summary="This is a table of the pages currently available for use in your application.">
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
	</div>
	<% Html.EndForm(); %>
    <% Html.Include("Footer"); %>
</body>
</html>
