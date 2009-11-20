<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%
    if (Request.IsAuthenticated) {
%>
Welcome <b>
    <%=Html.Encode(Page.User.Identity.Name)%></b>! [
<%=Html.ActionLink("Log Off", "LogOff", "Account", new { area = "" }, new { })%>
]
<%
    }
    else {
%>
[
<%=Html.ActionLink("Log On", "LogOn", "Account", new{area=""}, new{})%>
]
<%
    }
%>
