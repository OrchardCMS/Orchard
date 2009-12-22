<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<MediaItemEditViewModel>" %>
<%@ Import Namespace="Orchard.Media.Models"%>
<%@ Import Namespace="Orchard.Media.Helpers"%>
<%@ Import Namespace="Orchard.Media.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<h2>Edit Media - <%= Model.Name %></h2>
<p>
    <%=Html.ActionLink("Media Folders", "Index")%> &#62; 
    <%foreach (FolderNavigation navigation in MediaHelpers.GetFolderNavigationHierarchy(Model.MediaPath)) {%>
        <%=Html.ActionLink(navigation.FolderName, "Edit",
                  new {name = navigation.FolderName, mediaPath = navigation.FolderPath})%> &#62;
	    
    <% } %>
    Edit Media </p>
<div class="sections">
	<%using (Html.BeginFormAntiForgeryPost()) { %>
        <%= Html.ValidationSummary() %>
        <div class="primary">
		    <h3>About this media</h3>
            <fieldset>
                <label for="Name">Name:</label>
			    <input id="Name" name="Name" type="hidden" value="<%= Model.Name %>"/>
			    <input id="NewName" class="text" name="NewName" type="text" value="<%= Model.Name %>"/>
			    <label for="Caption">Caption:</label>
			    <input id="Caption" class="text" name="Caption" type="text" value="<%= Model.Caption %>"/>
			    <input id="LastUpdated" name="LastUpdated" type="hidden" value="<%= Model.LastUpdated %>"/>
			    <input id="Size" name="Size" type="hidden" value="<%= Model.Size %>"/>
			    <input id="FolderName" name="FolderName" type="hidden" value="<%= Model.FolderName %>"/>
			    <input id="MediaPath" name="MediaPath" type="hidden" value="<%= Model.MediaPath %>" />
			    <span>This will be used for the image alt tag.</span>
		    </fieldset>
		    <fieldset>
			    <input type="submit" class="button" name="submit.Save" value="Save" />
			    <%--<input type="submit" class="button" name="submit.Delete" value="Delete" />--%>
            </fieldset>
	    </div>
	    <div class="secondary">
		    <h3>Preview</h3>
		    <div><img src="<%=ResolveUrl("~/Media/" + Model.RelativePath + "/" + Model.Name)%>" class="previewImage" alt="<%= Model.Caption %>" /></div>
		    <ul>
			    <li><strong>Dimensions:</strong> 500 x 375 pixels</li>
			    <li><strong>Size:</strong> <%= Model.Size %></li>
			    <li><strong>Added on:</strong> <%= Model.LastUpdated %> by Orchard User</li>
			    <li>
			        <label for="embedPath">Embed:</label>
			        <input id="embedPath" class="inputText" name="embedPath" type="text" readonly="readonly" value="&lt;img src=&quot;<%=ResolveUrl("~/Media/" + Model.RelativePath + "/" + Model.Name)%>&quot; width=&quot;500&quot; height=&quot;375&quot; alt=&quot;<%= Model.Caption %>&quot; /&gt;" />
			        <p class="helperText">Copy this html to add this image to your site.</p>
			    </li>
		    </ul>
	    </div>
	<% } %>
</div>