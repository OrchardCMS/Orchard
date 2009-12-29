<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<AdminViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<% if (Model.CurrentUser != null) { //todo: (heskew) localize the string format "User: <username> | a:logoff"
    %><div id="login"><%="User"%>: <%=Html.Encode(Model.CurrentUser.UserName)%> | <%=Html.ActionLink("Logout", "LogOff", new { Area = "", Controller = "Account" }) %></div><%
   } %>