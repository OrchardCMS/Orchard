<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Core.Common.Settings.LocationSettingsViewModel>" %>
    <fieldset >
        <legend><%:T("{0}", Model.Definition.DisplayName) %></legend>

        <label for="<%:Html.FieldIdFor(m => m.Location.Zone) %>"><%:T("Zone name (e.g. body, primary)") %></label>
        <%:Html.EditorFor(m => m.Location.Zone)%>
        <%:Html.ValidationMessageFor(m => m.Location.Zone)%>

        <label for="<%:Html.FieldIdFor(m => m.Location.Position) %>"><%:T("Position in zone (e.g. 1, 1.0, 2.5.1)") %></label>
        <%:Html.EditorFor(m => m.Location.Position)%>
        <%:Html.ValidationMessageFor(m => m.Location.Position)%>
    </fieldset>
