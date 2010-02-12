<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<TagSettingsRecord>" %>
<%@ Import Namespace="Orchard.Tags.Models"%>
<fieldset>
    <legend><%=_Encoded("Tags")%></legend>
    <div>
        <%= Html.EditorFor(x=>x.EnableTagsOnPages) %>
        <label class="forcheckbox" for="TagSettings_EnableTagsOnPages">Pages can be tagged</label>
        <%= Html.ValidationMessage("EnableTagsOnPages", "*")%>
        <span class="hint forcheckbox"><%=_Encoded("In the admin, if the user has permission to.") %></span>
    </div>
</fieldset>