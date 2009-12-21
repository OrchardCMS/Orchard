<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<TagSettingsRecord>" %>
<%@ Import Namespace="Orchard.Tags.Models"%>
<h3>Tags</h3>
<ol>
    <li>
        <%= Html.LabelFor(x=>x.EnableTagsOnPages) %>
        <%= Html.EditorFor(x=>x.EnableTagsOnPages) %>
        <%= Html.ValidationMessage("EnableTagsOnPages", "*")%>
    </li>
</ol>
