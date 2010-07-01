<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.ContentTypes.ViewModels.EditPartFieldViewModel>" %>
    <fieldset class="manage-field">
        <h3><%:Model.Name %> <span>(<%:Model.FieldDefinition.Name %>)</span></h3>
        <div class="manage">
            <%:Html.Link("[remove]", "#forshowonlyandnotintendedtowork!") %>
        </div><%
        Html.RenderTemplates(Model.Templates); %>
        <%:Html.HiddenFor(m => m.Name) %><%:Html.HiddenFor(m => m.FieldDefinition.Name) %>
    </fieldset>