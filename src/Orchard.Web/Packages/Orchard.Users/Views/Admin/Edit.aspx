<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.Users.ViewModels.UserEditViewModel>" %>

<%@ Import Namespace="Orchard.Security" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<h2>
    Edit User</h2>
<%using (Html.BeginForm()) { %>
<%=Html.ValidationSummary() %>
<%=Html.EditorFor(m=>m.Id) %>
<%=Html.EditorFor(m=>m.UserName, "inputTextLarge") %>
<%=Html.EditorFor(m=>m.Email, "inputTextLarge") %>
<%=Html.EditorForItem(Model.User) %>
<fieldset>
    <input class="button" type="submit" value="Save" />
</fieldset>
<% } %>
