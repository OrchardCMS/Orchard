<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<NotifyEntry>>" %>
<%@ Import Namespace="Orchard.UI.Notify"%>

<script runat="server">
string CssClassName(NotifyType type) {
    switch(type) {
        case NotifyType.Error:
            return "critical";
        case NotifyType.Warning:
            return "warning";
    }
    return "informational";
}</script>

<% foreach (var item in Model) { %>
<div class="<%=CssClassName(item.Type) %>"><%=Html.Encode(item.Message) %></div>
<% } %>
