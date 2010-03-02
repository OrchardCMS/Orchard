<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<object>" %>
<h1><%=Html.TitleForPage(T("Change Password").ToString()) %></h1>
<p><%=_Encoded("Use the form below to change your password.")%></p>
<p><%=_Encoded("New passwords are required to be a minimum of {0} characters in length.", ViewData["PasswordLength"] as string) %></p>
<%=Html.ValidationSummary(T("Password change was unsuccessful. Please correct the errors and try again.").ToString())%>
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <fieldset>
        <legend><%=_Encoded("Account Information")%></legend>
        <div>
            <label for="currentPassword"><%=_Encoded("Current password:")%></label>
            <%=Html.Password("currentPassword") %>
            <%=Html.ValidationMessage("currentPassword") %>
        </div>
        <div>
            <label for="newPassword"><%=_Encoded("New password:")%></label>
            <%= Html.Password("newPassword") %>
            <%= Html.ValidationMessage("newPassword") %>
        </div>
        <div>
            <label for="confirmPassword"><%=_Encoded("Confirm new password:")%></label>
            <%= Html.Password("confirmPassword") %>
            <%= Html.ValidationMessage("confirmPassword") %>
        </div>
        <div>
            <input type="submit" value="<%=_Encoded("Change Password") %>" />
        </div>
    </fieldset>
<% } %>