<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<MediaItemAddViewModel>" %>
<%@ Import Namespace="Orchard.Media.Helpers"%>
<%@ Import Namespace="Orchard.Media.Models"%>
<%@ Import Namespace="Orchard.Media.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<% Html.Include("AdminHead"); %>
					<div class="yui-g">
						<h2 class="separator">Add Media </h2>
						<p class="bottomSpacer">
						<%=Html.ActionLink("Media Folders", "Index")%> &#62; 
						<%foreach (FolderNavigation navigation in MediaHelpers.GetFolderNavigationHierarchy(Model.MediaPath)) {%>
						    <%=Html.ActionLink(navigation.FolderName, "Edit",
                                      new {name = navigation.FolderName, mediaPath = navigation.FolderPath})%> &#62;
						    
						<% } %>
						Add Media</p>
						<div id="dialog" title="Upload files">        
                        <% using (Html.BeginForm("Add", "Admin", FormMethod.Post, new { enctype = "multipart/form-data" })) {%>
                            <ol>
                                <li><label for="pageTitle">File Path - Multiple files must be in a zipped folder:</label>
                                <input id="FolderName" name="FolderName" type="hidden" value="<%= Model.FolderName %>" />
                                <input id="MediaPath" name="MediaPath" type="hidden" value="<%= Model.MediaPath %>" />
                                <input id="MediaItemPath" name="MediaItemPath" type="file" class="button" value="Browse" size="64"/>
								<p class="helperText">After your files have been uploaded, you can edit the titles and descriptions.</p>
								</li>
								<li>
								<input type="submit" class="button" value="Upload" />
								</li>
						        </ol>    
                        <% } %>    
                        </div>
					</div>
<% Html.Include("AdminFoot"); %>