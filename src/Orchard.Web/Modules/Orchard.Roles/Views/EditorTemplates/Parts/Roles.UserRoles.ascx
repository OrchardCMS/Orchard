<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<UserRolesViewModel>" %>
<%@ Import Namespace="Orchard.Roles.ViewModels"%>
<fieldset>
    <legend><%: T("Roles")%></legend>
    <% if (Model.Roles.Count > 0) {
           var index = 0;
           foreach (var entry in Model.Roles) {
               if (string.Equals(entry.Name, "Authenticated", StringComparison.OrdinalIgnoreCase) || string.Equals(entry.Name, "Anonymous", StringComparison.OrdinalIgnoreCase)) {
                   continue;
               }%>
    <%: Html.Hidden("Roles[" + index + "].RoleId", entry.RoleId)%>
    <%: Html.Hidden("Roles[" + index + "].Name", entry.Name)%>
    <div>
    <%: Html.CheckBox("Roles[" + index + "].Granted", entry.Granted)%>
    <label class="forcheckbox" for="<%="Roles[" + index + "]_Granted"%>"><%: entry.Name %></label>
    </div>
    <%++index;
        }
       }
       else {
    %><p><%: T("There are no roles.")%></p><%
        } %>
</fieldset>
