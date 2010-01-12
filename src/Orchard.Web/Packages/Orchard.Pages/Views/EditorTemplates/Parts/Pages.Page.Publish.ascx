<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Pages.Models.Page>" %>
<fieldset>
    <legend><%=_Encoded("Publish Settings")%></legend>
    <div>
        <%=Html.RadioButton("Command", "SaveDraft", Model.ContentItem.VersionRecord == null || !Model.ContentItem.VersionRecord.Published, new { id = "Command_SaveDraft" }) %>
        <label class="forcheckbox" for="Command_SaveDraft"><%=_Encoded("Save Draft")%></label>
    </div>
    <div>
        <%=Html.RadioButton("Command", "PublishNow", Model.ContentItem.VersionRecord != null && Model.ContentItem.VersionRecord.Published, new { id = "Command_PublishNow" })%>
        <label class="forcheckbox" for="Command_PublishNow"><%=_Encoded("Publish Now")%></label>
    </div>
    <div>
        <%=Html.RadioButton("Command", "PublishLater", Model.Published != null && Model.Published.Value > DateTime.UtcNow, new { id = "Command_PublishLater" }) %>
        <label class="forcheckbox" for="Command_PublishLater"><%=_Encoded("Publish Later")%></label>
        <%=Html.EditorFor(m => m.Published) %>
    </div>
</fieldset>