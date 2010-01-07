<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Orchard.Pages.Models.Page>" %>
<fieldset>
    <legend>Publish Settings</legend>
    <label for="Command_SaveDraft"><%=Html.RadioButton("Command", "SaveDraft", true, new { id = "Command_SaveDraft" }) %> Save Draft</label><br />
</fieldset>
<fieldset>
    <label for="Command_PublishNow"><%=Html.RadioButton("Command", "PublishNow", new { id = "Command_PublishNow" }) %> Publish Now</label>
</fieldset>
<%--<fieldset>
    <label for="Command_PublishLater"><%=Html.RadioButton("Command", "PublishLater", new { id = "Command_PublishLater" }) %> Publish Later</label>
    <%=Html.EditorFor(m => m.Published) %>
</fieldset>--%>