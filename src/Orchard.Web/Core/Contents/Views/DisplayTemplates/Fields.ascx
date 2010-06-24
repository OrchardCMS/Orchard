<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<IEnumerable<EditPartFieldViewModel>>" %>
<%@ Import Namespace="Orchard.Core.Contents.ViewModels" %><%
if (Model.Any()) { %>
    <dl><%
        foreach (var field in Model) {
            var f = field; %>
            <%:Html.DisplayFor(m => f, "Field") %><%
        } %>
    </dl><%
} %>