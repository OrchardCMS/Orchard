<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<SandboxSettingsPartRecord>" %>
<%@ Import Namespace="Orchard.Sandbox.Models"%>
<fieldset>
    <legend>Sandbox</legend>
    <div>
        <%: Html.EditorFor(m => m.AllowAnonymousEdits) %>
        <label class="forcheckbox" for="SandboxSettings_AllowAnonymousEdits"><%: T("Anyone can create and edit pages") %></label>
        <%: Html.ValidationMessage("AllowAnonymousEdits", "*") %>
    </div>
</fieldset>
