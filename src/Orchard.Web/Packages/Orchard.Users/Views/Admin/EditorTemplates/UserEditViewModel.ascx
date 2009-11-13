<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Orchard.Users.ViewModels.UserEditViewModel>" %>
<%@ Import Namespace="Orchard.Utility" %>

<ol>
    <%=Html.EditorFor(m=>m.Id) %>
    <%=Html.EditorFor(m=>m.UserName, "inputTextLarge") %>
    <%=Html.EditorFor(m=>m.Email, "inputTextLarge") %>
</ol>

<% foreach(var editor in Model.Editors) {
     Html.RenderPartial(editor.PartialName, editor.ViewData);
} %>
