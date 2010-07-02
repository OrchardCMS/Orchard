<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<IEnumerable<Orchard.ContentTypes.ViewModels.EditTypePartViewModel>>" %>
<%
if (Model.Any()) { %>
    <fieldset><%
        var pi = 0;
        foreach (var part in Model) {
            var p = part;
            var htmlFieldName = string.Format("Parts[{0}]", pi++); %>
            <%:Html.EditorFor(m => p, "TypePart", htmlFieldName) %><%
        } %>
    </fieldset><%
} %>