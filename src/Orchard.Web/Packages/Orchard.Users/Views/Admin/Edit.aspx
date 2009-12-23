<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.Users.ViewModels.UserEditViewModel>" %>
<%@ Import Namespace="Orchard.Security" %>
<h2><%=Html.TitleForPage("Edit User") %></h2>
<%using (Html.BeginFormAntiForgeryPost()) { %>
    <%=Html.ValidationSummary() %>
    <%=Html.EditorFor(m=>m.Id) %>
    <%=Html.EditorFor(m=>m.UserName, "inputTextLarge") %>
    <%=Html.EditorFor(m=>m.Email, "inputTextLarge") %>
    <%=Html.EditorForItem(Model.User) %>
    <fieldset>
        <input class="button" type="submit" value="Save" />
    </fieldset>
<% } %>
