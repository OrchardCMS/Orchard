<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Orchard.Pages.Models.Page>" %>
<fieldset>
    <legend>Publish Settings</legend>
    <label for="Command_SaveDraft"><%=Html.RadioButton("Command", "SaveDraft", true, new { id = "Command_SaveDraft" }) %> Save Draft</label><br />
</fieldset>