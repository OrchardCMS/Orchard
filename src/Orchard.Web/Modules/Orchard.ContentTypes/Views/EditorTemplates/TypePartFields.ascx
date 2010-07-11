<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<IEnumerable<Orchard.ContentTypes.ViewModels.EditPartFieldViewModel>>" %>
<%
if (Model.Any()) {
    foreach (var field in Model) {
        var f = field; %>
        <%:Html.EditorFor(m => f, "TypePartField", f.Prefix) %><%
    }
} %>