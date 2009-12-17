<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<MediaItemAddViewModel>" %>
<%@ Import Namespace="Orchard.Media.Helpers"%>
<%@ Import Namespace="Orchard.Media.Models"%>
<%@ Import Namespace="Orchard.Media.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<h2>Add Media </h2>
<p>
    <%=Html.ActionLink("Media Folders", "Index")%> &#62; 
    <%foreach (FolderNavigation navigation in MediaHelpers.GetFolderNavigationHierarchy(Model.MediaPath)) {%>
        <%=Html.ActionLink(navigation.FolderName, "Edit",
                  new {name = navigation.FolderName, mediaPath = navigation.FolderPath})%> &#62;
	    
    <% } %>
    Add Media</p>
<% using (Html.BeginForm("Add", "Admin", FormMethod.Post, new { enctype = "multipart/form-data" })) {%>
    <%= Html.ValidationSummary() %>
    <fieldset>
        <label for="pageTitle">File Path - Multiple files must be in a zipped folder:</label>
        <input id="FolderName" name="FolderName" type="hidden" value="<%= Model.FolderName %>" />
        <input id="MediaPath" name="MediaPath" type="hidden" value="<%= Model.MediaPath %>" />
        <input id="MediaItemPath" name="MediaItemPath" type="file" class="text" value="Browse" size="64"/>
		<input type="submit" class="button" value="Upload" /><br />
		<span>After your files have been uploaded, you can edit the titles and descriptions.</span>
	</fieldset>
<% } %>