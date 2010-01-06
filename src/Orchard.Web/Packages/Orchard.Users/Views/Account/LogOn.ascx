<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<LogOnViewModel>" %>
<%@ Import Namespace="Orchard.Users.ViewModels"%>
<h1><%=Html.TitleForPage(Model.Title) %></h1>
<p><%=_Encoded("Please enter your username and password.")%> <%= Html.ActionLink("Register", "Register") %><%=_Encoded(" if you don't have an account.")%></p>
<%= Html.ValidationSummary(T("Login was unsuccessful. Please correct the errors and try again.").ToString()) %>
<% using (Html.BeginForm(new {Action="LogOn"})) { %>
    <fieldset>
        <legend><%=_Encoded("Account Information")%></legend>
            <label for="username"><%=_Encoded("Username:")%></label>
            <%= Html.TextBox("username") %>
            <%= Html.ValidationMessage("username") %>

            <label for="password"><%=_Encoded("Password:")%></label>
            <%= Html.Password("password") %>
            <%= Html.ValidationMessage("password") %>

            <%= Html.CheckBox("rememberMe") %><label class="forcheckbox" for="rememberMe"><%=_Encoded("Remember me?")%></label>

            <%=Html.HiddenFor(m=>m.ReturnUrl) %>
            <%=Html.AntiForgeryTokenOrchard() %>
            <input type="submit" value="<%=_Encoded("Log On") %>" />
    </fieldset>
<% } %>