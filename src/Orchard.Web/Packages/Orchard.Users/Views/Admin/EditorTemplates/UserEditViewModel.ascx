<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Orchard.Users.ViewModels.UserEditViewModel>" %>
<%@ Import Namespace="Orchard.Utility" %>
    <%=Html.EditorFor(m=>m.Id) %>
    <%=Html.EditorFor(m=>m.UserName, "inputTextLarge") %>
    <%=Html.EditorFor(m=>m.Email, "inputTextLarge") %>
<% foreach(var e in Model.EditorModel.Editors) {%>
     <%=Html.EditorFor(m => e.Model, e.TemplateName, e.Prefix)%>
<%} %>
