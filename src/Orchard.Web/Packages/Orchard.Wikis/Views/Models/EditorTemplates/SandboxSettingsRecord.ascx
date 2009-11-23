<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Orchard.Sandbox.Models.SandboxSettingsRecord>" %>
<h3>Sandbox</h3>
<ol>
    <li>
        <%= Html.LabelFor(x=>x.AllowAnonymousEdits) %>
        <%= Html.EditorFor(x=>x.AllowAnonymousEdits) %>
        <%= Html.ValidationMessage("AllowAnonymousEdits", "*")%>
    </li>
    <li>
        <%= Html.LabelFor(x => x.NameOfThemeWhenEditingPage)%>
        <%= Html.EditorFor(x=>x.NameOfThemeWhenEditingPage) %>
        <%= Html.ValidationMessage("NameOfThemeWhenEditingPage", "*")%>
    </li>
</ol>
