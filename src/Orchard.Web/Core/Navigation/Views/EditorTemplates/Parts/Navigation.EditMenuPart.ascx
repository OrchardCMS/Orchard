<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<MenuPart>" %>
<%@ Import Namespace="Orchard.Core.Navigation.Models"%>
<%@ Import Namespace="Orchard.Core.Navigation.ViewModels"%>
<fieldset>
    <%=Html.LabelFor(m => m.AddToMainMenu) %>
    <%=Html.EditorFor(m => m.AddToMainMenu) %>
    <%=Html.LabelFor(m => m.MenuText) %>
    <%=Html.TextBoxFor(m => m.MenuText, new { @class = "large text" })%>
</fieldset>