<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<MediaFolderEditPropertiesViewModel>" %>
<%@ Import Namespace="Orchard.Media.Helpers"%>
<%@ Import Namespace="Orchard.Media.Models"%>
<%@ Import Namespace="Orchard.Media.ViewModels"%>
<h1><%=Html.TitleForPage(T("Folder Properties").ToString())%></h1>
<p><%=Html.ActionLink(T("Media Folders").ToString(), "Index")%> &#62; 
    <%foreach (FolderNavigation navigation in MediaHelpers.GetFolderNavigationHierarchy(Model.MediaPath)) {%>
        <%=Html.ActionLink(navigation.FolderName, "Edit",
                  new {name = navigation.FolderName, mediaPath = navigation.FolderPath})%> &#62;
	    
    <% } %>
    <%=_Encoded("Folder Properties")%></p>
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <%=Html.ValidationSummary() %>
    <fieldset>
        <label for="Name"><%=_Encoded("Folder Name:") %></label>
		<input id="MediaPath" name="MediaPath" type="hidden" value="<%=Html.Encode(Model.MediaPath) %>" />
		<input id="Name" class="text" name="Name" type="text" value="<%=Html.Encode(Model.Name) %>" />
		<input type="submit" class="button" name="submit.Save" value="<%=_Encoded("Save") %>" />
		<%--<input type="submit" class="button buttonFocus roundCorners" name="submit.Delete" value="<%=_Encoded("Delete") %>" />--%>
    </fieldset>
<% } %>