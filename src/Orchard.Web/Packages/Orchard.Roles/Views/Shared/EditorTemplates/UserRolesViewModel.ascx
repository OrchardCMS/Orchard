<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Orchard.Roles.ViewModels.UserRolesViewModel>" %>
<fieldset>
    <legend>Roles</legend>
    <% if (Model.Roles.Count > 0) { 
        var index = 0;
        foreach (var entry in Model.Roles)
        {%>
        <%=Html.Hidden("Roles[" + index + "].RoleId", entry.RoleId)%>
        <%=Html.Hidden("Roles[" + index + "].Name", entry.Name)%>
        <label for="<%="Roles[" + index + "]_Granted"%>"><%= Html.CheckBox("Roles[" + index + "].Granted", entry.Granted)%> <%=Html.Encode(entry.Name)%></label>
        <%++index;
        }
       } else {
        %><p>There are no roles</p><%
       } %>