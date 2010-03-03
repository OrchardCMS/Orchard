<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<MediaFolderCreateViewModel>" %>
<%@ Import Namespace="Orchard.Media.Helpers"%>
<%@ Import Namespace="Orchard.Media.Models"%>
<%@ Import Namespace="Orchard.Media.ViewModels"%>
<h1><%=Html.TitleForPage(T("Add a Folder").ToString()) %></h1>
<div class="breadCrumbs">
<p><%=Html.ActionLink(T("Media Folders").ToString(), "Index") %> &#62; 
		<%foreach (FolderNavigation navigation in MediaHelpers.GetFolderNavigationHierarchy(Model.MediaPath)) { %>
		    <%=Html.ActionLink(navigation.FolderName, "Edit",
                      new {name = navigation.FolderName, mediaPath = navigation.FolderPath}) %> &#62;
		<% } %>
		<%=_Encoded("Add a Folder") %></p>
</div> 
			
<%using (Html.BeginFormAntiForgeryPost()) { %>
    <%= Html.ValidationSummary() %>
    <fieldset>
        <label for="Name"><%=_Encoded("Folder Name") %></label>
		<input id="Name" class="textMedium" name="Name" type="text" />
	    <input type="hidden" id="MediaPath" name="MediaPath" value="<%=Html.Encode(Model.MediaPath) %>" />
    </fieldset>
	<fieldset>
	    <input type="submit" class="button primaryAction" value="<%=_Encoded("Save") %>" />
    </fieldset>
 <% } %>