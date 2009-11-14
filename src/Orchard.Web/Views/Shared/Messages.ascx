<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<NotifyEntry>>" %>
<%@ Import Namespace="Orchard.UI.Notify"%>

<script runat="server">
string CssClassName(NotifyType type) {
    switch(type) {
        case NotifyType.Error:
            return "validation-summary-errors";
        case NotifyType.Warning:
            return "validation-summary-errors";
    }
    return "validation-summary-errors";
}</script>

<% foreach (var item in Model) { %>
<div class="<%=CssClassName(item.Type) %>"><%=Html.Encode(item.Message) %></div>
<% } %>
