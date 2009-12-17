<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.Users.ViewModels.UserCreateViewModel>" %>
<%@ Import Namespace="Orchard.Security" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<h2>Add User</h2>
<%using (Html.BeginForm()) { %>
    <%= Html.ValidationSummary() %>
    <%= Html.EditorForModel() %>
    <fieldset>
        <input class="button" type="submit" value="Create" />
    </fieldset>
<% } %>