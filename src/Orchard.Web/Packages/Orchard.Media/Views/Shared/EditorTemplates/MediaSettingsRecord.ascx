<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<MediaSettingsRecord>" %>
<%@ Import Namespace="Orchard.Media.Models"%>
<h3>Media</h3>
<ol>
    <li>
        <%= Html.LabelFor(x=>x.RootMediaFolder) %>
        <%= Html.EditorFor(x=>x.RootMediaFolder) %>
        <%= Html.ValidationMessage("RootMediaFolder", "*")%>
    </li>
</ol>
