<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<SandboxSettingsRecord>" %>
<%@ Import Namespace="Orchard.Sandbox.Models"%>
<fieldset>
    <legend>Sandbox</legend>
    <div>
        <%=Html.EditorFor(m => m.AllowAnonymousEdits) %>
        <label class="forcheckbox" for="SandboxSettings_AllowAnonymousEdits"><%=_Encoded("Anyone can create and edit pages") %></label>
        <%=Html.ValidationMessage("AllowAnonymousEdits", "*") %>
    </div>
</fieldset>
