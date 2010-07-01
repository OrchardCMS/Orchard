<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Core.Common.Fields.TextField>" %>
    <fieldset>
        <label for="<%:Html.FieldIdFor(m=>m.Value) %>"><%:Model.Name %></label>
        <%:Html.EditorFor(m=>m.Value) %><%:Html.ValidationMessageFor(m=>m.Value) %>
    </fieldset>