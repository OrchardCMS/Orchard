<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl" %>
<div class="Zone Zone-<%: (string) Model.ZoneName %>">
    <%foreach (var item in Model) {%><%:Display(item)%><%}%>
</div><!-- Zone-<%:(string)Model.ZoneName %> -->
