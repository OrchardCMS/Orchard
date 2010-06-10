<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<EditItemViewModel>" %>

<%@ Import Namespace="Orchard.Core.Contents.ViewModels" %>
<% Html.AddTitleParts(T("Edit Content").ToString()); %>
<% using (Html.BeginFormAntiForgeryPost()) { %>
<%:Html.ValidationSummary() %>
<%:Html.EditorForItem(m=>m.Content) %>
<%} %>
