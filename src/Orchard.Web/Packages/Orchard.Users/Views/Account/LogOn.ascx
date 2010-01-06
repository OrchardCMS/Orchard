<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<LogOnViewModel>" %>
<%@ Import Namespace="Orchard.Users.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<h2><%=Html.TitleForPage(Model.Title) %></h2>
<p>Please enter your username and password. <%= Html.ActionLink("Register", "Register") %> if you don't have an account.</p>
<%= Html.ValidationSummary("Login was unsuccessful. Please correct the errors and try again.") %>

<% using (Html.BeginForm(new {Action="LogOn"})) { %>
    <div>
        <fieldset>
            <legend>Account Information</legend>
            <p>
                <label for="username">Username:</label>
                <%= Html.TextBox("username") %>
                <%= Html.ValidationMessage("username") %>
            </p>
            <p>
                <label for="password">Password:</label>
                <%= Html.Password("password") %>
                <%= Html.ValidationMessage("password") %>
            </p>
            <p>
                <%= Html.CheckBox("rememberMe") %> <label class="inline" for="rememberMe">Remember me?</label>
            </p>
            <p>
                <%=Html.HiddenFor(m=>m.ReturnUrl) %>
                <%=Html.AntiForgeryTokenOrchard() %>
                <input type="submit" value="Log On" />
            </p>
        </fieldset>
    </div>
<% } %>