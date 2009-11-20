<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<AdminViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
        </div><%-- #main --%>
        <ul id="navigation" role="navigation">
            <li class="first"><h4>Dashboard</h4></li>
            <%if (Model.AdminMenu != null) {
                  foreach (var menuSection in Model.AdminMenu) {%>
                  <li><h4><%=Html.Encode(menuSection.Text)%></h4><ul><%foreach (var menuItem in menuSection.Items) { %>
                  <li><%=Html.ActionLink(menuItem.Text, (string)menuItem.RouteValues["action"], menuItem.RouteValues)%></li>
                  <%} %></ul></li>
            <%
                }
              }%>
        </ul>
    </div><%-- #content --%>
    <div id="footer" role="contentinfo"></div><%-- #contentinfo --%>
</body>
</html>