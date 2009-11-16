<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<MediaItemEditViewModel>" %>
<%@ Import Namespace="Orchard.Media.Models"%>
<%@ Import Namespace="Orchard.Media.Helpers"%>
<%@ Import Namespace="Orchard.Media.ViewModels"%>

<%@ Import Namespace="Orchard.Mvc.Html" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Edit Media File</title>
    <% Html.Include("Head"); %>
</head>
<body>
    <% Html.Include("Header"); %>
					<div class="yui-g">
						<h2 class="separator">Edit Media - <%= Model.Name %></h2>
						<p class="bottomSpacer">
						<%=Html.ActionLink("Media Folders", "Index")%> &#62; 
						<%foreach (FolderNavigation navigation in MediaHelpers.GetFolderNavigationHierarchy(Model.MediaPath)) {%>
						    <%=Html.ActionLink(navigation.FolderName, "Edit",
                                      new {name = navigation.FolderName, mediaPath = navigation.FolderPath})%> &#62;
						    
						<% } %>
						Edit Media </p>
					</div>
					<div class="yui-gc">
						<div class="yui-u first">
						<h3>About this media</h3>
							<%using (Html.BeginForm()) { %>
							<ol>
<%--								<li><label for="imageTitle">Title:</label>
								<input id="imageTitle" class="inputText inputTextLarge" name="imageTitle" type="text" />
								</li>--%>
								<li><label for="Name">Name:</label>
								<input id="Name" class="inputText inputTextLarge" name="Name" type="hidden" value="<%= Model.Name %>"/>
								<input id="NewName" class="inputText inputTextLarge" name="NewName" type="text" value="<%= Model.Name %>"/>
								</li>
								<li><label for="Caption">Caption:</label>
								<input id="Caption" class="inputText" name="Caption" type="text" value="<%= Model.Caption %>"/>
								<input id="LastUpdated" class="inputText" name="LastUpdated" type="hidden" value="<%= Model.LastUpdated %>"/>
								<input id="Size" class="inputText" name="Size" type="hidden" value="<%= Model.Size %>"/>
								<input id="FolderName" class="inputText" name="FolderName" type="hidden" value="<%= Model.FolderName %>"/>
								<input id="MediaPath" class="inputText" name="MediaPath" type="hidden" value="<%= Model.MediaPath %>"
								<p class="helperText">This will be used for the image alt tag.</p>
								</li>
<%--								<li><label for="tags">Tags:</label>
								<input id="tags" class="inputText" name="tags" type="text" />
								<p class="helperText">Seperate each tag with a comma. Example: Lorem, Ipsum, Dolor</p>
								</li>--%>
<%--								<li><label for="description">Description:</label>
								<textarea id="description" name="description"></textarea>
								</li>--%>
								<li>
								<input type="submit" class="button" name="submit.Save" value="Save" />
								<input type="submit" class="button" name="submit.Delete" value="Delete" />
								</li>
							</ol>
							<%}%>
						</div>
						<div class="yui-u sideBar">
							<h3>Preview</h3>
							<fieldset>
							<ol>
								<li>
								<img src="<%=ResolveUrl("~/Media/" + Model.RelativePath + "/" + Model.Name)%>" class="previewImage" alt="<%= Model.Caption %>" />
								</li>
								<li>
								<strong>Dimensions:</strong> 500 x 375 pixels
								</li>
								<li>
								<strong>Size:</strong> <%= Model.Size %>
								</li>
								<li>
								<strong>Added on:</strong> <%= Model.LastUpdated %> by Orchard User
								</li>
								<li>
								<label for="embedPath">Embed:</label>
								<input id="embedPath" class="inputText" name="embedPath" type="text" readonly="readonly" value="&lt;img src=&quot;<%=ResolveUrl("~/Media/" + Model.RelativePath + "/" + Model.Name)%>&quot; width=&quot;500&quot; height=&quot;375&quot; alt=&quot;<%= Model.Caption %>&quot; /&gt;" />
								<p class="helperText">Copy this html to add this image to your site.</p>
								</li>
							</ol>
							</fieldset>
						</div>
					</div>
    <% Html.Include("Footer"); %>
</body>
</html>
