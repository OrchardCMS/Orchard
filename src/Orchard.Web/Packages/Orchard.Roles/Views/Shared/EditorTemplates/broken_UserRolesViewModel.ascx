<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Orchard.Roles.ViewModels.UserRolesViewModel>" %>
<h3>
    Roles</h3>
<ol>
<%
    var index = 0; foreach (var entry in Model.Roles) {%>
<li>
    
    <%= Html.Hidden("Roles[" + index + "].RoleId", entry.RoleId)%>
    
    <label for="<%="Roles[" + index + "]_Granted"%>"><%= Html.CheckBox("Roles[" + index + "].Granted", entry.Granted)%> <%=Html.Encode(entry.Name)%></label>
</li>
<%++index;
    } %>
    </ol>
    
