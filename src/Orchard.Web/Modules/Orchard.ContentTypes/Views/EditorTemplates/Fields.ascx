<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<IEnumerable<Orchard.ContentTypes.ViewModels.EditPartFieldViewModel>>" %>
<%@ Import Namespace="Orchard.Core.Contents.ViewModels" %><%
if (Model.Any()) { %>
    <fieldset><%
        var fi = 0;
        foreach (var field in Model) {
            var f = field;
            var htmlFieldName = string.Format("Fields[{0}]", fi++); %>
            <%:Html.EditorFor(m => f, "Field", htmlFieldName) %><%
        } %>
    </fieldset><%
} %>