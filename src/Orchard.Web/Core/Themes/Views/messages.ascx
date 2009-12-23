<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<NotifyEntry>>" %>
<%@ Import Namespace="Orchard.UI.Notify"%>
<%-- todo: (heskew) not this --%>
<script runat="server">
string CssClassName(NotifyType type) {
    switch(type) {
        case NotifyType.Error:
            return "critical";
        case NotifyType.Warning:
            return "warning";
    }
    return "info";
}</script>
<% foreach (var item in Model) { %>
<div class="<%=CssClassName(item.Type) %> message"><%=Html.Encode(item.Message) %></div>
<% } %>