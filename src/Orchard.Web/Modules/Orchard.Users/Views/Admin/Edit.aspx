<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<UserEditViewModel>" %>
<%@ Import Namespace="Orchard.Users.ViewModels"%>
<h1><%=Html.TitleForPage(T("Edit User").ToString()) %></h1>
<%using (Html.BeginFormAntiForgeryPost()) { %>
    <%=Html.ValidationSummary() %>
    <%=Html.EditorFor(m=>m.Id) %>
    <%=Html.EditorFor(m=>m.UserName, "inputTextLarge") %>
    <%=Html.EditorFor(m=>m.Email, "inputTextLarge") %>
    <%=Html.EditorForItem(Model.User) %>
    <fieldset>
        <input class="button primaryAction" type="submit" value="<%=_Encoded("Save") %>" />
    </fieldset>
<% } %>
