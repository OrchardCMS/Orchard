<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<AdminViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
            </div><%-- .wrapper --%>
        </div><%-- #main --%>
        <ul id="navigation" role="navigation">
            <li class="first"><h4><span>Dashboard</span></h4></li>
            <%if (Model.AdminMenu != null) {
                  foreach (var menuSection in Model.AdminMenu) {
                      // todo: (heskew) need some help(er)
                      var firstSectionItem = menuSection.Items.FirstOrDefault();
                      var sectionHeaderMarkup = firstSectionItem != null
                          ? Html.ActionLink(menuSection.Text, (string)firstSectionItem.RouteValues["action"], firstSectionItem.RouteValues).ToHtmlString()
                          : string.Format("<span>{0}</span>", Html.Encode(menuSection.Text));
                      %>
                  <li><h4><%=sectionHeaderMarkup%></h4><ul><%foreach (var menuItem in menuSection.Items) { %>
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