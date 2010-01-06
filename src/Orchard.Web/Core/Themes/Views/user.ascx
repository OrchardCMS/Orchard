<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<div id="logindisplay">
<% if (Request.IsAuthenticated) { %> 
    Welcome <strong><%=Html.Encode(Page.User.Identity.Name) %></strong>! 
    [<%=Html.ActionLink("Log Off", "LogOff", new { Controller = "Account", Area = "Orchard.Users" })%>]
<% } else { %>
    [<%=Html.ActionLink("Log On", "LogOn", new { Controller = "Account", Area = "Orchard.Users", ReturnUrl = Context.Request.RawUrl })%>]
<% } %>
</div>
