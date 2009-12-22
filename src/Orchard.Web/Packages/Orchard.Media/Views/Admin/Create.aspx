<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<MediaFolderCreateViewModel>" %>
<%@ Import Namespace="Orchard.Media.Models"%>
<%@ Import Namespace="Orchard.Media.Helpers"%>
<%@ Import Namespace="Orchard.Media.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<h2>Add a Folder</h2>
<p><%=Html.ActionLink("Media Folders", "Index")%> &#62; 
		<%foreach (FolderNavigation navigation in MediaHelpers.GetFolderNavigationHierarchy(Model.MediaPath)) {%>
		    <%=Html.ActionLink(navigation.FolderName, "Edit",
                      new {name = navigation.FolderName, mediaPath = navigation.FolderPath})%> &#62;
		<% } %>
		Add a Folder</p>
<%using (Html.BeginFormAntiForgeryPost()) { %>
    <%= Html.ValidationSummary() %>
    <fieldset>
        <label for="Name">Folder Name:</label>
		<input id="Name" class="text" name="Name" type="text" />
		<input id="MediaPath" name="MediaPath" type="hidden" value="<%= Model.MediaPath %>" />
	    <input type="submit" class="button" value="Save" />
    </fieldset>
 <% } %>