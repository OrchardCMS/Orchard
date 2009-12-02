<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<MediaFolderEditPropertiesViewModel>" %>
<%@ Import Namespace="Orchard.Media.Models"%>
<%@ Import Namespace="Orchard.Media.Helpers"%>
<%@ Import Namespace="Orchard.Media.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<% Html.Include("AdminHead"); %>
                    <div class="yui-g">
						<h2 class="separator">Folder Properties</h2>
						<p class="bottomSpacer">
						<%=Html.ActionLink("Media Folders", "Index")%> &#62; 
						<%foreach (FolderNavigation navigation in MediaHelpers.GetFolderNavigationHierarchy(Model.MediaPath)) {%>
						    <%=Html.ActionLink(navigation.FolderName, "Edit",
                                      new {name = navigation.FolderName, mediaPath = navigation.FolderPath})%> &#62;
						    
						<% } %>
						Folder Properties
						</p>
						        <%using (Html.BeginForm()) { %>
                                <%= Html.ValidationSummary() %>
                                <ol>
								<li><label for="Name">Folder Name:</label>
								<input id="Name" class="inputText inputTextLarge roundCorners" name="Name" type="text" value="<%= Model.Name %>" />
								<input id="MediaPath" name="MediaPath" type="hidden" value="<%=Model.MediaPath %>" />
								</li>
								<li>
								<input type="submit" class="button buttonFocus roundCorners" name="submit.Save" value="Save" />
								<input type="submit" class="button buttonFocus roundCorners" name="submit.Delete" value="Delete" />
								</li>
						        </ol>
                                <%}%>
					</div>
<% Html.Include("AdminFoot"); %>