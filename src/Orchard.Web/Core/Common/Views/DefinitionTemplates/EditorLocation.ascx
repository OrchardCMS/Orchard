<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Core.Common.Settings.LocationSettings>" %>
    <fieldset >
        <legend><%:T("Editor Location") %></legend>

        <label for="<%:Html.FieldIdFor(m => m.Zone) %>"><%:T("Zone name (e.g. body, primary)")%></label>
        <%:Html.EditorFor(m=>m.Zone) %>
        <%:Html.ValidationMessageFor(m => m.Zone)%>

        <label for="<%:Html.FieldIdFor(m => m.Position) %>"><%:T("Position in zone (e.g. 1, 1.0, 2.5.1)") %></label>
        <%:Html.EditorFor(m=>m.Position) %>
        <%:Html.ValidationMessageFor(m => m.Position)%>
    </fieldset>
