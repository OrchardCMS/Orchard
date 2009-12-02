<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<SettingsIndexViewModel>" %>
<%@ Import Namespace="Orchard.Core.Settings.ViewModels"%>
<%@ Import Namespace="Orchard.Utility" %>

<h3>Global Settings</h3>
<ol>
<%=Html.EditorFor(s=>s.Id) %>
    <li>
        <%= Html.LabelFor(x=>x.SiteName) %>
        <%= Html.EditorFor(x=>x.SiteName) %>
        <%= Html.ValidationMessage("SiteName", "*")%>
    </li>
    <li>
        <%= Html.LabelFor(x => x.SuperUser)%>
        <%= Html.EditorFor(x=>x.SuperUser) %>
        <%= Html.ValidationMessage("SuperUser", "*")%>
    </li>
</ol>


<% foreach(var e in Model.ItemView.Editors) {%>
     <%=Html.EditorFor(m => e.Model, e.TemplateName, e.Prefix)%>
<%} %>
