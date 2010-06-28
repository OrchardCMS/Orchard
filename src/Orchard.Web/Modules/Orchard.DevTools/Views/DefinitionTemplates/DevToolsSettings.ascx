<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.DevTools.Settings.DevToolsSettings>" %>
<fieldset>
    <%:Html.EditorFor(m=>m.ShowDebugLinks) %>
    <label for="<%:Html.FieldIdFor(m => m.ShowDebugLinks) %>" class="forcheckbox"><%:T("Show debug links") %></label>
    <%:Html.ValidationMessageFor(m=>m.ShowDebugLinks) %>
</fieldset>
