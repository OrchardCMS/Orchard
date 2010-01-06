<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<object>" %>
<h1><%=Html.TitleForPage(T("Create a New Account").ToString()) %></h1>
<p><%=_Encoded("Use the form below to create a new account.")%></p>
<p><%=_Encoded("Passwords are required to be a minimum of {0} characters in length.", ViewData["PasswordLength"] as string)%></p>
<%=Html.ValidationSummary(T("Account creation was unsuccessful. Please correct the errors and try again.").ToString()) %>
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <fieldset>
        <legend><%=_Encoded("Account Information")%></legend>
        <fieldset>
            <label for="username"><%=_Encoded("Username:")%></label>
            <%= Html.TextBox("username") %>
            <%= Html.ValidationMessage("username") %>
        </fieldset>
        <fieldset>
            <label for="email"><%=_Encoded("Email:")%></label>
            <%= Html.TextBox("email") %>
            <%= Html.ValidationMessage("email") %>
        </fieldset>
        <fieldset>
            <label for="password"><%=_Encoded("Password:")%></label>
            <%= Html.Password("password") %>
            <%= Html.ValidationMessage("password") %>
        </fieldset>
        <fieldset>
            <label for="confirmPassword"><%=_Encoded("Confirm password:")%></label>
            <%= Html.Password("confirmPassword") %>
            <%= Html.ValidationMessage("confirmPassword") %>
        </fieldset>
        <fieldset>
            <input type="submit" value="<%=_Encoded("Register") %>" />
        </fieldset>
    </fieldset>
<% } %>