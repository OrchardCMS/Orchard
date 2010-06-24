<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.ContentTypes.ViewModels.ListContentTypesViewModel>" %>

<h1>
    <%:Html.TitleForPage(T("Content Types").ToString())%></h1>
<div class="manage">
    <%: Html.ActionLink(T("Create new type").ToString(), "Create", new{Controller="Admin",Area="Orchard.ContentTypes"}, new { @class = "button primaryAction" })%></div>
<%:Html.UnorderedList(
    Model.Types,
    (t,i) => Html.DisplayFor(m => t),
    "contentItems"
    ) %>
