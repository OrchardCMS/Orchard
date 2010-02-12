<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<MenuPart>" %>
<%@ Import Namespace="Orchard.Core.Navigation.Models"%>
<%@ Import Namespace="Orchard.Core.Navigation.ViewModels"%>
<fieldset>
    <%=Html.EditorFor(m => m.OnMainMenu) %>
    <label for="OnMainMenu" class="forcheckbox"><%=_Encoded("Add to the main menu") %></label>
    <label for="MenuText"><%=_Encoded("Menu text") %></label>
    <%=Html.TextBoxFor(m => m.MenuText, new { @class = "large text" })%>
</fieldset>