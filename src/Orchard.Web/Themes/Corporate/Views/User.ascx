<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<object>" %>
<ul class="admin group">
    <% if (Request.IsAuthenticated)
       { %>
        <li><%=_Encoded("Welcome")%> <%=Html.Encode(Page.User.Identity.Name) %></li>
        <li><%=Html.ActionLink(T("Log Off").ToString(), "LogOff", new { Controller = "Account", Area = "Orchard.Users" })%></li>
        <li><%= Html.ActionLink("Admin", "Index", new {Area = "Dashboard", Controller = "Admin"})%></li>
    <% }
       else
       { %>
        <li><%=Html.ActionLink(T("Login").ToString(), "LogOn", new { Controller = "Account", Area = "Orchard.Users", ReturnUrl = Context.Request.RawUrl })%></li>
    <% } %>
</ul>