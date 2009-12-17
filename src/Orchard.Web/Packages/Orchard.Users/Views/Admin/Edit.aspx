<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.Users.ViewModels.UserEditViewModel>" %>
<%@ Import Namespace="Orchard.Security" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<h2>Edit User</h2>
<%using (Html.BeginForm()) { %>
<ol>
    <%= Html.ValidationSummary() %>
    <%= Html.EditorForModel() %>
    <fieldset>
        <input class="button" type="submit" value="Save" /> 
    </fieldset>
<% } %>