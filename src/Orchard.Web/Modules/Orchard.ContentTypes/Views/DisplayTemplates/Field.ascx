<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.ContentTypes.ViewModels.EditPartFieldViewModel>" %>
    <dt><%:Model.Name %> <span>(<%:Model.FieldDefinition.Name %>)</span></dt>
    <dd>
        <%:Html.DisplayFor(m => m.Settings, "Settings", "") %>
    </dd>