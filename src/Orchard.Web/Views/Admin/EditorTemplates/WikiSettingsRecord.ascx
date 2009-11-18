<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Orchard.Wikis.Models.WikiSettingsRecord>" %>
<h3>Wiki</h3>
<ol>
    <li>
        <%= Html.LabelFor(x=>x.AllowAnonymousEdits) %>
        <%= Html.EditorFor(x=>x.AllowAnonymousEdits) %>
        <%= Html.ValidationMessage("AllowAnonymousEdits", "*")%>
    </li>
    <li>
        <%= Html.LabelFor(x => x.WikiEditTheme)%>
        <%= Html.EditorFor(x=>x.WikiEditTheme) %>
        <%= Html.ValidationMessage("WikiEditTheme", "*")%>
    </li>
</ol>
