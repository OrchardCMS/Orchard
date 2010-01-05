<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Orchard.Sandbox.Models.SandboxSettingsRecord>" %>
<fieldset>
    <legend>Sandbox</legend>
    <%= Html.LabelFor(x=>x.AllowAnonymousEdits) %>
    <%= Html.EditorFor(x=>x.AllowAnonymousEdits) %>
    <%= Html.ValidationMessage("AllowAnonymousEdits", "*")%>
    <br />
</fieldset>
