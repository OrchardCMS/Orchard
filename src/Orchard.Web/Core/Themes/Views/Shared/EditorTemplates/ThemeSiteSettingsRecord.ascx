<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ThemeSiteSettingsRecord>" %>
<%@ Import Namespace="Orchard.Core.Themes.Records"%>
<h3>Themes</h3>
<ol>
    <li>
        <%= Html.LabelFor(x=>x.CurrentThemeName) %>
        <%= Html.EditorFor(x=>x.CurrentThemeName) %>
        <%= Html.ValidationMessage("CurrentThemeName", "*")%>
    </li>
</ol>
