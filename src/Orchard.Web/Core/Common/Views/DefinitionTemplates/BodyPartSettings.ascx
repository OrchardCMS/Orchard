<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Core.Common.Settings.BodyPartSettings>" %>
<fieldset>
    <label for="<%:Html.FieldIdFor(m => m.FlavorDefault) %>"><%:T("Default flavor") %></label>
    <%:Html.EditorFor(m => m.FlavorDefault)%>
    <%:Html.ValidationMessageFor(m => m.FlavorDefault)%>
</fieldset>
