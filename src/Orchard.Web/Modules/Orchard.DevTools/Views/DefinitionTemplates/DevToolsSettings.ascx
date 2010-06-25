<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Orchard.DevTools.Settings.DevToolsSettings>" %>
<fieldset>
    <%:Html.LabelFor(m=>m.ShowDebugLinks) %>
    <%:Html.EditorFor(m=>m.ShowDebugLinks) %>
    <%:Html.ValidationMessageFor(m=>m.ShowDebugLinks) %>
</fieldset>
