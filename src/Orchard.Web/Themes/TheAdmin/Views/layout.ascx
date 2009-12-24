<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<AdminViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%
Html.RegisterStyle("site.css"); %>
<div id="header" role="banner">
    <h1><%=Html.ActionLink("Project Orchard", "Index", new { Area = "", Controller = "Home" })%></h1>
    <div id="site"><%=Html.ActionLink("Your Site", "Index", new { Area = "", Controller = "Home" })%></div>
    <% if (Model.CurrentUser != null) { //todo: (heskew) localize the string format "User: <username> | a:logoff"
        %><div id="login"><%="User"%>: <%=Html.Encode(Model.CurrentUser.UserName)%> | <%=Html.ActionLink("Logout", "LogOff", new { Area = "", Controller = "Account" }) %></div><%
        } %>
</div>
<div id="content">
    <div id="navshortcut"><a href="#navigation"><%="Skip to navigation" %></a></div>
    <div id="main" role="main">
        <div class="wrapper">
            <% Html.RenderPartial("Messages", Model.Messages); %>
        </div><%
        Html.ZoneBody("content");
%>    </div>
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
</div>
<div id="footer" role="contentinfo"><%
    Html.Zone("footer");
%></div>