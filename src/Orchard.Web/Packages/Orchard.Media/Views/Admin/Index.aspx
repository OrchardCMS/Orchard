<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<MediaFolderIndexViewModel>" %>
<%@ Import Namespace="Orchard.Media.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<% Html.Include("Header"); %>
    <% Html.BeginForm(); %>
    <div class="yui-g">
						<h2 class="separator">Manage Media Folders</h2>
						<p class="bottomSpacer">
						<%=Html.ActionLink("Media Folders", "Index")%> &#62; 
						Manage Media Folders</p>
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
						    <%=Html.ActionLink("Add a new folder", "Create") %>
						</span>
						<table id="Table1" cellspacing="0" class="roundCorners clearLayout" summary="This is a table of the media folders currently available for use in your application.">
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
			                <%foreach (var mediaFolder in Model.MediaFolders) {
                            %>
                            <tr>
                                <td><input type="checkbox" value="true" name="<%= "Checkbox." + mediaFolder.Name %>"/></td>
                                <td><img src="<%=ResolveUrl("~/Packages/Orchard.Media/Content/Admin/images/folder.gif")%>" height="16px" width="16px" class="mediaTypeIcon" alt="Folder" />
                                    <%=Html.ActionLink(mediaFolder.Name, "Edit", new { name = mediaFolder.Name, mediaPath = mediaFolder.MediaPath })%>
                                </td>
                                <td>Orchard User</td>
			                    <td><%=mediaFolder.LastUpdated %></td>
				                <td>Folder</td>
				                <td><%=mediaFolder.Size %></td>
                            </tr>
                            <%}%>
				        </table>
	</div>
	<% Html.EndForm(); %>
<% Html.Include("Footer"); %>