<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<MediaFolderEditPropertiesViewModel>" %>
<%@ Import Namespace="Orchard.Media.Models"%>
<%@ Import Namespace="Orchard.Media.Helpers"%>
<%@ Import Namespace="Orchard.Media.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<h2>Folder Properties</h2>
<p><%=Html.ActionLink("Media Folders", "Index")%> &#62; 
    <%foreach (FolderNavigation navigation in MediaHelpers.GetFolderNavigationHierarchy(Model.MediaPath)) {%>
        <%=Html.ActionLink(navigation.FolderName, "Edit",
                  new {name = navigation.FolderName, mediaPath = navigation.FolderPath})%> &#62;
	    
    <% } %>
    Folder Properties</p>
<%using (Html.BeginForm()) { %>
    <%= Html.ValidationSummary() %>
    <fieldset>
        <label for="Name">Folder Name:</label>
		<input id="MediaPath" name="MediaPath" type="hidden" value="<%=Model.MediaPath %>" />
		<input id="Name" class="text" name="Name" type="text" value="<%= Model.Name %>" />
		<input type="submit" class="button buttonFocus roundCorners" name="submit.Save" value="Save" />
		<%--<input type="submit" class="button buttonFocus roundCorners" name="submit.Delete" value="Delete" />--%>
    </fieldset>
<%}%>