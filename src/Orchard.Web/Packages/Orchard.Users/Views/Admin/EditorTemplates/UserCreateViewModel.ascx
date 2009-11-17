<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Orchard.Users.ViewModels.UserCreateViewModel>" %>
<%@ Import Namespace="Orchard.Utility" %>

<ol>
    <%=Html.EditorFor(m=>m.UserName, "inputTextLarge") %>
    <%=Html.EditorFor(m=>m.Email, "inputTextLarge") %>
    <%=Html.EditorFor(m=>m.Password, "inputPasswordLarge") %>
    <%=Html.EditorFor(m=>m.ConfirmPassword, "inputPasswordLarge") %>
</ol>
