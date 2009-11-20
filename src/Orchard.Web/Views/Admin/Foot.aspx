<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<AdminViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
        </div><%-- #content --%>
        <%-- Navigation --%>
        <div id="navigation">
            <div class="leftNavMod">
                <h4>Dashboard</h4>
            </div>
            <%if (Model.AdminMenu != null) {
                  foreach (var menuSection in Model.AdminMenu) {%>
                  <div class="leftNavMod"><h4><%=Html.Encode(menuSection.Text)%></h4><ul><%foreach (var menuItem in menuSection.Items) { %>
                  <li><%=Html.ActionLink(menuItem.Text, (string)menuItem.RouteValues["action"], menuItem.RouteValues)%></li>
                  <%} %></ul></div>
            <%
                }
              }%>
        </div>
    </div><%-- #main --%>
</body>
</html>