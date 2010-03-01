<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<object>" %>
<h1><%=Html.TitleForPage(T("Create a New Account").ToString()) %></h1>
<p><%=_Encoded("Use the form below to create a new account.")%></p>
<p><%=_Encoded("Passwords are required to be a minimum of {0} characters in length.", ViewData["PasswordLength"] as string)%></p>
<%=Html.ValidationSummary(T("Account creation was unsuccessful. Please correct the errors and try again.").ToString()) %>
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <fieldset>
        <legend><%=_Encoded("Account Information")%></legend>
        <div>
            <label for="username"><%=_Encoded("Username:")%></label>
            <%= Html.TextBox("username") %>
            <%= Html.ValidationMessage("username") %>
        </div>
        <div>
            <label for="email"><%=_Encoded("Email:")%></label>
            <%= Html.TextBox("email") %>
            <%= Html.ValidationMessage("email") %>
        </div>
        <div>
            <label for="password"><%=_Encoded("Password:")%></label>
            <%= Html.Password("password") %>
            <%= Html.ValidationMessage("password") %>
        </div>
        <div>
            <label for="confirmPassword"><%=_Encoded("Confirm password:")%></label>
            <%= Html.Password("confirmPassword") %>
            <%= Html.ValidationMessage("confirmPassword") %>
        </div>
        <div>
            <input type="submit" value="<%=_Encoded("Register") %>" />
        </div>
    </fieldset>
<% } %>