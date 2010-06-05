<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<MediaItemAddViewModel>" %>
<%@ Import Namespace="Orchard.Media.Helpers"%>
<%@ Import Namespace="Orchard.Media.Models"%>
<%@ Import Namespace="Orchard.Media.ViewModels"%><%
Html.RegisterStyle("admin.css"); %>
<h1><%=Html.TitleForPage(T("Add Media").ToString()) %></h1>
<div class="breadCrumbs">
<p><%=Html.ActionLink(T("Media Folders").ToString(), "Index") %> &#62; 
    <%foreach (FolderNavigation navigation in MediaHelpers.GetFolderNavigationHierarchy(Model.MediaPath)) { %>
        <%=Html.ActionLink(navigation.FolderName, "Edit",
                  new {name = navigation.FolderName, mediaPath = navigation.FolderPath})%> &#62;
	    
    <% } %>
    <%=_Encoded("Add Media") %></p>
</div>

<% using (Html.BeginForm("Add", "Admin", FormMethod.Post, new { enctype = "multipart/form-data" })) { %>
    <%= Html.ValidationSummary() %>
    <fieldset>
        <label for="pageTitle"><%=T("File Path <span> - multiple files must be in a zipped folder</span>")%></label>
        <input id="MediaItemPath" name="MediaItemPath" type="file" class="text" value="<%=_Encoded("Browse") %>" size="64"/>
		<span class="hint"><%=_Encoded("After your files have been uploaded, you can edit the titles and descriptions.")%></span>
        <input type="hidden" id="FolderName" name="FolderName" value="<%=Html.Encode(Model.FolderName) %>" />
        <input type="hidden" id="MediaPath" name="MediaPath" value="<%=Html.Encode(Model.MediaPath) %>" />
    </fieldset>
    <fieldset>
		<input type="submit" class="button primaryAction" value="<%=_Encoded("Upload") %>" />
		<%=Html.AntiForgeryTokenOrchard() %>
	</fieldset>
<% } %>