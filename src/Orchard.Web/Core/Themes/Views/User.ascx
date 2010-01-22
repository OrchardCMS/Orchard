<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<object>" %>
<div id="logindisplay">
<% if (Request.IsAuthenticated) { %> 
    <%=_Encoded("Welcome")%> <strong><%=Html.Encode(Page.User.Identity.Name) %></strong>! 
    [<%=Html.ActionLink(T("Log Off").ToString(), "LogOff", new { Controller = "Account", Area = "Orchard.Users" })%>]
<% } else { %>
    [<%=Html.ActionLink(T("Log On").ToString(), "LogOn", new { Controller = "Account", Area = "Orchard.Users", ReturnUrl = Context.Request.RawUrl })%>]
<% } %>
</div>