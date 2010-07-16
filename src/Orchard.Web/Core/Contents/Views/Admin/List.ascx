<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Core.Contents.ViewModels.ListContentsViewModel>" %>
<h1><%:Html.TitleForPage((string.IsNullOrEmpty(Model.TypeDisplayName) ? T("Manage Content") : T("Manage {0} Content", Model.TypeDisplayName)).ToString())%></h1>
<div class="manage">
    <%:Html.ActionLink(!string.IsNullOrEmpty(Model.TypeDisplayName) ? T("Add new {0} content", Model.TypeDisplayName).Text : T("Add new content").Text, "Create", new { }, new { @class = "button primaryAction" })%>
</div>
<%:Html.UnorderedList(
    Model.Entries,
    (entry, i) => Html.DisplayForItem(entry.ViewModel),
    "contentItems") %>