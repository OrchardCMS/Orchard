<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl" %>
<div class="Zone Zone-<%:Model.ZoneName %>">
    <%foreach (var item in Model) {%><%:Display(item)%><%}%>
</div><!-- Zone-<%:Model.ZoneName %> -->
