<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<UserCreateViewModel>" %>
<%@ Import Namespace="Orchard.Users.ViewModels"%>
<h1><%=Html.TitleForPage(T("Add User").ToString()) %></h1>
<%using (Html.BeginFormAntiForgeryPost()) { %>
    <%=Html.ValidationSummary() %>
    <%=Html.EditorFor(m=>m.UserName, "inputTextLarge") %>
    <%=Html.EditorFor(m=>m.Email, "inputTextLarge") %>
    <%=Html.EditorFor(m=>m.Password, "inputPasswordLarge") %>
    <%=Html.EditorFor(m=>m.ConfirmPassword, "inputPasswordLarge") %>
    <%=Html.EditorForItem(Model.User) %>
    <fieldset>
        <input class="button" type="submit" value="<%=_Encoded("Create") %>" />
    </fieldset>
<% } %>
