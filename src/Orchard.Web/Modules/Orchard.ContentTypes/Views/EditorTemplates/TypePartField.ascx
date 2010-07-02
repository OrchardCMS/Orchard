<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.ContentTypes.ViewModels.EditPartFieldViewModel>" %>
    <fieldset class="manage-field">
        <h4><%:Model.Name %> <span>(<%:Model.FieldDefinition.Name %>)</span></h4><%
        if (Model.Templates.Any()) { %>
        <div class="settings"><%
        Html.RenderTemplates(Model.Templates); %>
        </div><%
        } %>
        <%:Html.HiddenFor(m => m.Name) %><%:Html.HiddenFor(m => m.FieldDefinition.Name) %>
    </fieldset>