<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.ContentTypes.ViewModels.ListContentTypesViewModel>" %>
<% Html.RegisterStyle("admin.css"); %>
<h1><%:Html.TitleForPage(T("Manage Content Types").ToString())%></h1>
<div class="manage">
    <%:Html.ActionLink(T("Create new type").ToString(), "Create", new{Controller="Admin",Area="Orchard.ContentTypes"}, new { @class = "button primaryAction" }) %>
    <%:Html.ActionLink(T("Content Parts").ToString(), "ListParts", new{Controller="Admin",Area="Orchard.ContentTypes"}, new { @class = "button" }) %>
</div>
<%:Html.UnorderedList(
    Model.Types,
    (t,i) => Html.DisplayFor(m => t),
    "contentItems"
    ) %>