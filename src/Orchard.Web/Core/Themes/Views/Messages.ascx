<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<NotifyEntry>>" %>
<%@ Import Namespace="Orchard.UI.Notify"%>
<% foreach (var item in Model) {
     var className = item.Type == NotifyType.Error
                         ? "critical"
                         : item.Type == NotifyType.Warning
                               ? "warning"
                               : "info"; %>
<div class="<%=className %> message"><%=Html.Encode(item.Message) %></div>
<% } %>