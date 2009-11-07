<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<MediaFolderCreateViewModel>" %>
<%@ Import Namespace="Orchard.Media.Models"%>
<%@ Import Namespace="Orchard.Media.Helpers"%>
<%@ Import Namespace="Orchard.Media.ViewModels"%>

<%@ Import Namespace="Orchard.Mvc.Html" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Create Media Folder</title>
    <% Html.Include("Head"); %>
</head>
<body>
    <% Html.Include("Header"); %>
                    <div class="yui-g">
						<h2 class="separator">Add a New Folder</h2>
						<p class="bottomSpacer">
						<%=Html.ActionLink("Media Folders", "Index")%> &#62; 
						<%foreach (FolderNavigation navigation in MediaHelpers.GetFolderNavigationHierarchy(Model.MediaPath)) {%>
						    <%=Html.ActionLink(navigation.FolderName, "Edit",
                                      new {name = navigation.FolderName, mediaPath = navigation.FolderPath})%> &#62;
						    
						<% } %>
						Add a new Folder</p>
						        <%using (Html.BeginForm()) { %>
                                <%= Html.ValidationSummary() %>
                                <ol>
								<li><label for="Name">Folder Name:</label>
								<input id="Name" class="inputText inputTextLarge roundCorners" name="Name" type="text" />
								<input id="MediaPath" name="MediaPath" type="hidden" value="<%= Model.MediaPath %>" />
								</li>
								<li>
								<input type="submit" class="button buttonFocus roundCorners" value="Save" />
								</li>
						        </ol>
                                <%}%>
					</div>
    <% Html.Include("Footer"); %>
</body>
</html>
