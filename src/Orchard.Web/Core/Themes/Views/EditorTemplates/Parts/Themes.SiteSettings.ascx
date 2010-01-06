<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ThemeSiteSettingsRecord>" %>
<%@ Import Namespace="Orchard.Core.Themes.Records"%>
<fieldset>
    <legend><%=_Encoded("Themes")%></legend>
    <fieldset>
        <%= Html.LabelFor(x=>x.CurrentThemeName) %>
        <%= Html.EditorFor(x=>x.CurrentThemeName) %>
        <%= Html.ValidationMessage("CurrentThemeName", "*")%>
    </fieldset>
</fieldset>