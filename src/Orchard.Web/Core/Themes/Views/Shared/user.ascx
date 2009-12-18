<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<div id="logindisplay"><%
if (Request.IsAuthenticated) {
    %>Welcome <strong><%=Html.Encode(Page.User.Identity.Name) %></strong>! [<%=Html.ActionLink("Log Off", "LogOff", "Account", new { area = "" }, new { }) %>]<%
} else {
    %>[<%=Html.ActionLink("Log On", "LogOn", "Account", new{area=""}, new{}) %>]<%
}
%></div>