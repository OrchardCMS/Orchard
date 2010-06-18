<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ListContentTypesViewModel>" %>
<%@ Import Namespace="Orchard.Core.Contents.ViewModels" %>
<h1><%:Html.TitleForPage(T("Content Types").ToString())%></h1>
<div class="manage"><%: Html.ActionLink(T("Create new type").ToString(), "CreateType", null, new { @class = "button primaryAction" })%></div>
<%=Html.UnorderedList(
    Model.Types,
    (t,i) => Html.DisplayFor(m => t),
    "contentItems"
    ) %>