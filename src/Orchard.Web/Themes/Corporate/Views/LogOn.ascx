<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<LogOnViewModel>" %>
<%@ Import Namespace="Orchard.Users.ViewModels"%>

<h1 class="page-title"><%=Html.TitleForPage(Model.Title)%></h1>
<p><%=_Encoded("Please enter your username and password.")%> <%= Html.ActionLink("Register", "Register")%><%=_Encoded(" if you don't have an account.")%></p>
<%= Html.ValidationSummary(T("Login was unsuccessful. Please correct the errors and try again.").ToString())%>
<%
using (Html.BeginFormAntiForgeryPost(Url.Action("LogOn", new {ReturnUrl = Request.QueryString["ReturnUrl"]}))) { %>
<fieldset class="login-form">
    <legend><%=_Encoded("Account Information")%></legend>
    <div class="group">
         <label for="userNameOrEmail"><%=_Encoded("Username or Email:")%></label>
         <%= Html.TextBox("userNameOrEmail", "", new { autofocus = "autofocus" })%>
         <%= Html.ValidationMessage("userNameOrEmail")%>
    </div>
    <div class="group">
        <label for="password"><%=_Encoded("Password:")%></label>
        <%= Html.Password("password")%>
        <%= Html.ValidationMessage("password")%>
    </div>
    <div class="group">
        <%= Html.CheckBox("rememberMe")%><label class="forcheckbox" for="rememberMe"><%=_Encoded("Remember me?")%></label>
    </div>
    <input type="submit" value="<%=_Encoded("Log On") %>" />
</fieldset><%
} %>