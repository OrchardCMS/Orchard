<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<% if (Model.CurrentUser != null) {
    %><div id="login"><%=_Encoded("User:")%> <%=Html.Encode(Model.CurrentUser.UserName)%> | <%=Html.ActionLink(T("Logout").ToString(), "LogOff", new { Area = "Orchard.Users", Controller = "Account" }) %></div><%
   } %>