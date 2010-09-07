<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl" %>
<div class="zone zone-<%:Model.ZoneName %>">
    <%foreach (var item in Model) {%><%:Display(item)%><%}%>
</div>
