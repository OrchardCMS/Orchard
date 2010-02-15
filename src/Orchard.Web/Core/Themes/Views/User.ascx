<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<div id="logindisplay">
<% if (Request.IsAuthenticated) { %> 
    <%=T("Welcome, <strong>{0}</strong>!", Html.Encode(Page.User.Identity.Name)) %> 
    <%=Html.Link(T("Admin").ToString(), "/admin/blogs") %> /
    <%=Html.ActionLink(T("Log Off").ToString(), "LogOff", new { Controller = "Account", Area = "Orchard.Users" }) %>
<% } else { %>
    <%=Html.ActionLink(T("Log On").ToString(), "LogOn", new { Controller = "Account", Area = "Orchard.Users", ReturnUrl = Context.Request.RawUrl }) %>
<% } %>
</div>