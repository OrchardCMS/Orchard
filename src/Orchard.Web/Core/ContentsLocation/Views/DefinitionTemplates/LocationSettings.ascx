<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Core.ContentsLocation.ViewModels.LocationSettingsViewModel>" %>
<%
    Html.RegisterStyle("admin.css"); %>
    <fieldset class="location-setting">
        <legend><%:T("{0}", Model.Definition.DisplayName) %></legend>
        <fieldset>
            <label for="<%:Html.FieldIdFor(m => m.Location.Zone) %>"><%:T("Zone name (e.g. body, primary)") %></label><%
        if (!string.IsNullOrWhiteSpace(Model.DefaultLocation.Zone)) {
            %><span class="default"><%:T(" - default: {0}", Model.DefaultLocation.Zone) %></span><%
        } %><%:Html.EditorFor(m => m.Location.Zone) %><%:Html.ValidationMessageFor(m => m.Location.Zone)%>
        </fieldset>
        <fieldset>
            <label for="<%:Html.FieldIdFor(m => m.Location.Position) %>"><%:T("Position in zone (e.g. 1, 1.0, 2.5.1)") %></label><%
        if (!string.IsNullOrWhiteSpace(Model.DefaultLocation.Zone)) {
            %><span class="default"><%:T(" - default: {0}", Model.DefaultLocation.Position) %></span><%
        } %><%:Html.EditorFor(m => m.Location.Position) %>
        </fieldset><%:Html.ValidationMessageFor(m => m.Location.Position)%>
    </fieldset>