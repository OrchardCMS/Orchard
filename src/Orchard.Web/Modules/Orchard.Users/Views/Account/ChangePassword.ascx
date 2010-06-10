<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<object>" %>
<h1><%: Html.TitleForPage(T("Change Password").ToString()) %></h1>
<p><%: T("Use the form below to change your password.")%></p>
<p><%: T("New passwords are required to be a minimum of {0} characters in length.", ViewData["PasswordLength"] as string) %></p>
<%: Html.ValidationSummary(T("Password change was unsuccessful. Please correct the errors and try again.").ToString())%>
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <fieldset>
        <legend><%: T("Account Information")%></legend>
        <div>
            <label for="currentPassword"><%: T("Current password:")%></label>
            <%: Html.Password("currentPassword") %>
            <%: Html.ValidationMessage("currentPassword") %>
        </div>
        <div>
            <label for="newPassword"><%: T("New password:")%></label>
            <%: Html.Password("newPassword") %>
            <%: Html.ValidationMessage("newPassword") %>
        </div>
        <div>
            <label for="confirmPassword"><%: T("Confirm new password:")%></label>
            <%: Html.Password("confirmPassword") %>
            <%: Html.ValidationMessage("confirmPassword") %>
        </div>
        <div>
            <input type="submit" value="<%: T("Change Password") %>" />
        </div>
    </fieldset>
<% } %>